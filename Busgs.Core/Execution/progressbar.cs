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

namespace Execution
{
    public class ProgressBar
    {
        private char doneChar;
        private char undoneChar;
        private char leftBorder;
        private char rightBorder;
        private int width;

        public ProgressBar(
            int width,
            char doneChar = '#', char undoneChar = ' ',
            char leftBorder = '|', char rightBorder = '|'
        )
        {
            if (width < 10)
                throw new ArgumentException($"progress bar width {width} < 10");

            this.doneChar = doneChar;
            this.undoneChar = undoneChar;
            this.leftBorder = leftBorder;
            this.rightBorder = rightBorder;

            this.width = width;
        }

        public void Update(double progress)
        {
            if (progress < 0.0)
                throw new ArgumentException($"progress {progress} < 0");
            if (progress > 1.0)
                throw new ArgumentException($"progress {progress} > 1.0");
            printProgress(progress);
        }

        public void Finish()
        {
            printProgress(1.0);
            Console.Write("\n");
        }

        private void printProgress(double progress)
        {
            int doneCount = (int)(progress * width);
            string doneBar = new string(doneChar, doneCount);
            string undoneBar = new string(undoneChar, width - doneCount);
            Console.Write(
                $"\r{leftBorder}{doneBar}{undoneBar}{rightBorder} {(progress * 100):F1}%"
            );
        }
    }
}