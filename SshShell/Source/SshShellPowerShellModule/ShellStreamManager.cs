namespace C60.SshShellPowerShellModule
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;

    /// <summary>
    /// Wrap the ShellStream class and add read/write methods that appropriate for Shell session.
    /// </summary>
    public class ShellStreamManager
    {
        private StreamWriter writer;
        private StreamReader reader;

        public ShellStreamManager(Stream shellStream)
        {
            this.DefaultMillisecondsWaitForResponse = 10 * 1000;
            this.reader = new StreamReader(shellStream);
            this.writer = new StreamWriter(shellStream);
            //this.reader = new StreamReader(shellStream,new System.Text.UTF8Encoding());
            //this.writer = new StreamWriter(shellStream, new System.Text.UTF8Encoding());
            this.writer.AutoFlush = true;
        }

        /// <summary>
        /// Gets or sets the default time to wait for input data before throwing a timeout exception
        /// </summary>
        public int DefaultMillisecondsWaitForResponse { get; set; }

        public string[] ReadStreamWhileInputAvailable(int timeoutMilliseconds, string expect = null, string waitUnlimitedOn = null)
        {
            return this.ReadStreamWhileInputAvailable(TimeSpan.FromMilliseconds(timeoutMilliseconds), expect == null ? null : new Regex(expect), waitUnlimitedOn == null ? null : new Regex(waitUnlimitedOn));
        }

        /// <summary>
        /// Read the input stream while input available. 
        /// By default the method will finish immediately as there is no more input available. 
        /// The different parameters allow control to specify when to stop waiting for more input.
        /// </summary>
        /// <param name="timeout">Time to wait for more input. The time is measured after the last input was received.</param>
        /// <param name="expect">Cancel the wait if the last input line match to this regular expression.</param>
        /// <param name="waitUnlimitedOn">Wait for unlimited time when the last input line match this regular expression</param>
        /// <returns>Strings array with the last result</returns>
        public string[] ReadStreamWhileInputAvailable(TimeSpan timeout = default(TimeSpan), Regex expect = null, Regex waitUnlimitedOn = null)
        {
            List<string> result = new List<string>();
            bool noNeedToWaite = false;
            bool waitUnlimitedExpressionMatched = false;
            bool waitUnlimitedExpressionWaited = false;
            string line = this.reader.ReadLine();
            while (true)
            {
                if (line == null)
                {
                    if (noNeedToWaite || (timeout.TotalMilliseconds == 0 && !waitUnlimitedExpressionMatched))
                    {
                        break;
                    }
                    else
                    {
                        if (waitUnlimitedExpressionMatched)
                        {
                            waitUnlimitedExpressionWaited = true;
                            if (timeout.TotalMilliseconds == 0)
                            {
                                timeout = TimeSpan.FromMilliseconds(200);
                            }
                        }
                        else
                        {
                            noNeedToWaite = true;
                        }

                        Thread.Sleep(timeout);
                    }
                }
                else
                {
                    if (waitUnlimitedExpressionWaited)
                    {
                        waitUnlimitedExpressionMatched = false;
                        waitUnlimitedExpressionWaited = false;
                    }

                    result.Add(line);
                    if (expect != null && expect.IsMatch(line))
                    {
                        noNeedToWaite = true;
                    }

                    if (waitUnlimitedOn != null && waitUnlimitedOn.IsMatch(line))
                    {
                        waitUnlimitedExpressionMatched = true;
                    }
                }

                line = this.reader.ReadLine();
            }

            return result.ToArray();
        }
        public void JustWrite(string cmd)
        {
            this.writer.WriteLine(cmd);
        }
        public void WriteLineAndWaitForResponse(string line)
        {
            //Console.WriteLine("Stream Manager {0}",line.Length.ToString());

            //line.Replace(line, line + "\r\n");


            //this.writer.WriteLine(line);

            //line.Replace(line, line + System.Environment.NewLine);
            //Console.Write(line);
            //this.writer.WriteLine("N0tP@$$sw0rd!");
            this.writer.Write(line);
            //this.writer.Write(System.Environment.NewLine);
            this.writer.Write("\r");
            //this.writer.Dispose();
            //this.writer.Close();
            //this.writer.Flush();
            //this.WriteLineAndWaitForResponse(line);
            //this.WaitForResponse();
            //this.writer.BaseStream.Position = 0;
            //this.writer.WriteLine("N0tP@$$sw0rd!");
            //this.writer.Flush();
            //this.WaitForResponse();
            //string temp = "N0tP@$$sw0rd!";
            //temp.Replace(temp, temp + System.Environment.NewLine);
            //this.writer.Write(temp);
            //this.writer.Flush();

             this.WaitForResponse();
            //this.writer.BaseStream.Position = 0;
        }

        public void WaitForResponse(int timeoutMilliseconds)
        {
            this.WaitForResponse(TimeSpan.FromMilliseconds(timeoutMilliseconds));
        }

        public void WaitForResponse(TimeSpan? timeout = null)
        {
            double waitIterations = timeout.HasValue ? timeout.Value.TotalMilliseconds / 500 : this.DefaultMillisecondsWaitForResponse / 500;
            int currentWaitIteration = 0;
            while (this.reader.BaseStream.Length == 0 && currentWaitIteration < waitIterations)
            {
                //Console.WriteLine()
                Thread.Sleep(500);
                currentWaitIteration++;
            }

            if (currentWaitIteration >= waitIterations && this.reader.BaseStream.Length == 0)
            {
                throw new TimeoutException(string.Format("Time out, no response after {0} seconds", waitIterations / 2));
            }
        }
    }
}
