namespace C60.SshShellPowerShellModule
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Remove noise character from SSH output result.
    /// </summary>
    public static class OutputResultCleaner
    {
        private static readonly Regex NoiseSections = new Regex("[^\u0008]+?[\u0008]+");

        /// <summary>
        /// Inside a noise section there are still valid characters after the $ sign
        /// </summary>
        private static readonly Regex ValidCharactersFilter = new Regex(@"(?<=[^\$]*?[\$])[^\u0008]*");
            
        /// <summary>
        /// This clean method is based on experiment only with CISCO ASA FW. 
        /// The output stream can contain noise characters, special when the input is long. 
        /// The method remove the \u0008 (BS) characters and take the non-overlap characters after the $ sign.
        /// </summary>
        /// <param name="input">input to clean</param>
        /// <returns>clean result</returns>
        public static string CleanInvalidCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            string result = NoiseSections.Replace(
                input,
                (m) =>
                {   // Should ignore characters before the $ sign if exist.
                    int validCharactersStartIndex = m.Value.IndexOf('$');
                    string validCharacters;
                    if (validCharactersStartIndex != -1)
                    {
                        validCharacters = ValidCharactersFilter.Match(m.Value).Value.TrimEnd();
                    }
                    else
                    {
                        validCharacters = m.Value.Trim('\u0008').TrimEnd();
                    }

                    // The valid characters can overlap with the string before the noise. The for loop find the overlap index.
                    int overlapIndex = 0;
                    for (int i = validCharacters.Length; i > 0; i--)
                    {
                        if (input.Substring(0, m.Index).Trim('\u0008').EndsWith(validCharacters.Substring(0, i)))
                        {
                            overlapIndex = i;
                            break;
                        }
                    }

                    return validCharacters.Substring(overlapIndex);
                }).Trim('\u0008');

            return result;
        }
    }
}
