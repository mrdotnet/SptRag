// File: Program.cs

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace WebhookCLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Webhook CLI Utility")
        {
            new Option<string>("--payload", "Path to payload JSON file") { IsRequired = true },
            new Option<string>("--key", "Path to private key PEM") { IsRequired = true },
            new Option<string>("--schema", "Path to JSON Schema file for validation") { IsRequired = true },
            new Option<string>("--output", "Output file path") { IsRequired = true },
            new Option<bool>("--send", "Send webhook after generating")
        };

        rootCommand.Handler = CommandHandler.Create<string, string, string, string, bool>(async (payload, key, schema, output, send) =>
        {
            try
            {
                var rawJson = await File.ReadAllTextAsync(payload);
                JsonSchemaValidator.Validate(rawJson, schema);

                var encrypted = PayloadEncryptor.Encrypt(rawJson);
                var signed = WebhookSigner.Sign(encrypted, key);

                await File.WriteAllTextAsync(output, signed);
                Console.WriteLine("Signed and encrypted payload written to: " + output);

                if (send)
                {
                    await WebhookSender.SendAsync(signed);
                    Console.WriteLine("Webhook sent.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex.Message);
            }
        });

        return await rootCommand.InvokeAsync(args);
    }
}