using System;
using System.IO;
using ElectionGuard.Decryption.ElectionRecord;
using ElectionGuard.Decryption.Verify;

namespace ElectionGuard.CLI.Encrypt
{
    /// <summary>
    /// Verify an Election Record.
    /// </summary>
    internal class VerifyCommand
    {
        public static Task Execute(VerifyOptions options)
        {
            try
            {
                var command = new VerifyCommand();
                return command.ExecuteInternal(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private async Task ExecuteInternal(VerifyOptions options)
        {
            options.Validate();

            if (string.IsNullOrEmpty(options.ZipFile))
            {
                throw new ArgumentNullException(nameof(options.ZipFile));
            }

            var electionRecord = await ElectionRecordManager.ImportAsync(options.ZipFile);
            var reuslts = await VerifyElection.VerifyAsync(electionRecord);

            Console.WriteLine(reuslts);

            Console.WriteLine($"All checks are complete. The election record is {(reuslts.AllValid ? "valid" : "invalid")}");
        }
    }
}
