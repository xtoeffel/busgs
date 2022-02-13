/* **********************************************************************
 Copyright 2022 Christof Dittmar
 
 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at
 
     http://www.apache.org/licenses/LICENSE-2.0
 
 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
********************************************************************** */

using System.Collections.Generic;
using Parameters;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;
using System;
using BaseIO;
using System.Linq;
using SimpleTable;

namespace ExcelIO
{
    public static class ExcelReader
    {
        private static List<string> GetColumnNames(List<List<object>> rawData, TableHeader tblHeader)
        {
            if (rawData.Count <= tblHeader.RowCount)
            {
                throw new ArgumentException(
                    $"Insufficient row count {rawData.Count} for header row count {tblHeader.RowCount}, " +
                    "row count must be header + 1"
                );
            }

            List<object> columnNames = rawData[tblHeader.RowColumnNames];
            foreach (object obj in columnNames)
            {
                if (obj.GetType() != typeof(string))
                {
                    throw new ArgumentException(
                        $"{obj} is {obj.GetType().Name} but expected string"
                    );
                }
            }

            return columnNames.Select(obj => obj.ToString()).ToList();
        }

        public static List<RequestParametersInput> Read(string file, string sheetName, TableHeader tableHeader)
        {
            return ReadFromStream(new FileStream(file, FileMode.Open, FileAccess.Read), sheetName, tableHeader);
        }
        

        public static List<RequestParametersInput> ReadFromStream(Stream stream, string sheetName, TableHeader tableHeader)
        {
            IWorkbook book;
            using (stream)
            {
                // TODO: does this read into memory or is it kept open until using is exited?
                book = new XSSFWorkbook(stream);
            }

            List<List<object>> table = new List<List<object>>();

            ISheet sheet = book.GetSheet(sheetName);

            if (sheet.FirstRowNum != 0)
            {
                throw new ArgumentException("Top left cell \"A1\" is empty: expected table there");
            }

            for (int nRow = 0; nRow <= sheet.LastRowNum; nRow++)
            {
                IRow row = sheet.GetRow(nRow);
                List<object> tableRow = new List<object>();
                table.Add(tableRow);
                for (int nCol = 0; nCol < row.LastCellNum; nCol++)
                {
                    ICell cell = row.GetCell(nCol);
                    object value;
                    var valueType = cell.CellType;
                    switch (valueType)
                    {
                        case CellType.Blank:
                            value = null;
                            break;
                        case CellType.Boolean:
                            value = cell.BooleanCellValue;
                            break;
                        case CellType.Numeric:
                            value = cell.NumericCellValue;
                            break;
                        case CellType.String:
                            value = cell.StringCellValue;
                            break;
                        default:
                            throw new ArgumentException(
                                $"Cell \"{sheetName}!{cell.Address.ToString()}\" contains " +
                                "unsupported value of type {valueType.ToString()}"
                            );
                    }
                    tableRow.Add(value);
                }
            }

            List<string> columnNames = GetColumnNames(table, tableHeader);
            List<List<object>> pureData = table.GetRange(tableHeader.RowCount, table.Count - tableHeader.RowCount);

            List<RequestParametersInput> parameters = new List<RequestParametersInput>();
            foreach (var row in pureData)
            {
                // TODO: find out why Tuple.Create is required and columnNames.zip(row).ToDictionary() didn't work
                var dict_ = columnNames.Zip(row, Tuple.Create).ToDictionary(t => t.Item1, t => t.Item2);
                parameters.Add(RequestParametersInput.FromDict(dict_));
            }
            return parameters;
        }
    }

    public static class ExcelWriter
    {
        private static void SetCellValue(ICell cell, object value)
        {
            if (value == null)
                return;
            else if (value.GetType() == typeof(string))
                cell.SetCellValue((string)value);
            else if (value.GetType() == typeof(int))
                cell.SetCellValue((int)value);
            else if (value.GetType() == typeof(double))
                cell.SetCellValue((double)value);
            else if (value.GetType() == typeof(bool))
                cell.SetCellValue((bool)value);
            else
                throw new ArgumentException($"Unsupported value type {value.GetType()}, failed to set cell value");
        }

        private static void WriteToSheet(
            IWorkbook book, 
            ISheet sheet, 
            SimpleDataTable dataTable
        )
        {
            if (dataTable.IsEmpty)
                return;
            if (dataTable.Header.Count() != 1)
                throw new NotImplementedException(
                    $"Unable to write data table: found {dataTable.Header.Count()} header rows, supported is 1"
                );

            IFont headerFont = book.CreateFont();
            headerFont.IsBold = true;
            var headerStyle = book.CreateCellStyle();
            headerStyle.SetFont(headerFont);

            var excelHeaderRow = sheet.CreateRow(0);
            // TODO: this could be multiple header rows, could be separate method WriteMatrixToSheet(book, sheet, array[,])
            foreach (var headerCell in dataTable.Header[0].Select((Value, Index) => new { Index, Value }))
            {
                var excelHeaderCell = excelHeaderRow.CreateCell(headerCell.Index);
                excelHeaderCell.CellStyle = headerStyle;
                excelHeaderCell.SetCellValue(headerCell.Value);
            }

            int rowOffset = dataTable.Header.Count();
            foreach (var dataRow in dataTable.Body.Select((Cells, Index) => new { Index, Cells }))
            {
                var excelRow = sheet.CreateRow(dataRow.Index + rowOffset);
                foreach (var cell in dataRow.Cells.Select((Value, Index) => new { Index, Value }))
                {
                    var excelCell = excelRow.CreateCell(cell.Index);
                    SetCellValue(excelCell, cell.Value);
                }
            }
        }

        /// <summary>
        /// Writes simple data table to excel file.
        /// </summary>
        public static void Write(
            string file, IDictionary<string, SimpleDataTable> data
        )
        {
            WriteToStream(new FileStream(file, FileMode.Create), data);
        }

        /// <summary>
        /// Writes simple data table to excel file.
        /// </summary>
        public static void WriteToStream(
            Stream stream, IDictionary<string, SimpleDataTable> data
        )
        {
            IWorkbook book = new XSSFWorkbook(XSSFWorkbookType.XLSX);
            var sortedKeys = data.Keys.OrderBy(key => key);
            foreach (var key in sortedKeys)
            {
                var sheet = book.CreateSheet(key);
                WriteToSheet(book, sheet, data[key]);
            }

            using (stream)
            {
                book.Write(stream);
            }
        }
    }
}