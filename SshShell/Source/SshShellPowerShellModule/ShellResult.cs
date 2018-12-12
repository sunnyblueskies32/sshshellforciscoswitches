namespace C60.SshShellPowerShellModule
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Accumulate SSH session result and provide properties to access different parts of the result.
    /// </summary>
    public class ShellResult
    {
        private List<string[]> allResult;

        public ShellResult()
        {
            this.allResult = new List<string[]>();
        }

        public string AllResult
        {
            get
            {
                return AggregateLines(this.allResult.SelectMany(r => r));
            }
        }

        public string LastResult
        {
            get
            {
                return AggregateLines(this.LastResultLines);
            }
        }

        public string LastResultLine
        {
            get
            {
                return this.LastResultLines.LastOrDefault() ?? string.Empty;
            }
        }

        public string LastResultWithoutFirstLastLines
        {
            get
            {
                if (this.LastResultLines.Count() < 3)
                {
                    return string.Empty;
                }
                else
                {
                    return AggregateLines(this.LastResultLines.Skip(1).Take(this.LastResultLines.Count() - 2));
                }
            }
        }

        private IEnumerable<string> LastResultLines
        {
            get
            {
                return this.allResult.LastOrDefault() ?? new string[0];
            }
        }

        public void AddResult(string result, bool cleanInvalidCharacters = true)
        {
            this.AddResult(new string[] { result }, cleanInvalidCharacters);
        }

        public void AddResult(string[] result, bool cleanInvalidCharacters = true)
        {
            if (result != null && result.Length != 0 && !(result.Length == 1 && string.IsNullOrEmpty(result[0])))
            {
                if (cleanInvalidCharacters)
                {
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = OutputResultCleaner.CleanInvalidCharacters(result[i]);
                    }
                }

                var lastResult = this.allResult.LastOrDefault();
                if (lastResult != null && lastResult.Length > 0)
                {
                    string lastLine = lastResult[lastResult.Length - 1];
                    lastResult[lastResult.Length - 1] = null;
                    result[0] = lastLine + result[0];
                }
                
                this.allResult.Add(result);
            }
        }

        public void ClearResult()
        {
            this.allResult = new List<string[]>();
        }

        private static string AggregateLines(IEnumerable<string> lines)
        {
            return lines.Where(r => r != null).Aggregate(new StringBuilder(), (a, s) => a.Append(s).Append('\n'), a => a.ToString().TrimEnd('\n'));
        }
    }
}
