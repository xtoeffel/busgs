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

using System;
using System.IO;


namespace BaseIO
{
    public class FileOverwriteException : ArgumentException
    {
        public FileOverwriteException(string message) : base(message) { }
    }

    static class Utils
    {
        public static void TestFileFolder(string file)
        {
            string folder = Path.GetDirectoryName(file);
            if (String.IsNullOrEmpty(folder))
            {
                throw new DirectoryNotFoundException("Undefined directory");
            }
            if (!Directory.Exists(folder))
            {
                throw new DirectoryNotFoundException($"Directory \"{folder}\" does not exist");
            }
        }
    }

    public class TableHeader
    {
        int _rowCount;
        int _rowColumnNames;

        public TableHeader(int rowCount, int indexRowOfColNames)
        {
            if (rowCount < 0)
            {
                throw new ArgumentException($"invalid row-count {rowCount}: expected >= 0");
            }
            if (indexRowOfColNames < 0)
            {
                throw new ArgumentException($"invalid row-of-names {indexRowOfColNames}: expected >= 0");
            }
            if (indexRowOfColNames > rowCount)
            {
                throw new ArgumentException(
                    $"invalid row-of-names {indexRowOfColNames}: expected <= {rowCount} (row-count)"
                );
            }

            this._rowCount = rowCount;
            this._rowColumnNames = indexRowOfColNames;
        }

        public int RowCount => this._rowCount;
        public int RowColumnNames => this._rowColumnNames;
    }
}