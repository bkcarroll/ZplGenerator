var generator = new ZplGenerator(suppressCrLf: false);

var zpl = generator.Start()
                   .DrawBoxAt(50, 50, 100, 100, 2)
                   .SetFont(50)
                   .WriteTextAt(150, 150, "Hello World")
                   .AddQrCodeAt(200, 200, "Hello World")
                   .End()
                   .ToString();

TextCopy.ClipboardService.SetText(zpl);

Console.Write($"Copied to clipboard:\r\n\r\n{zpl}\r\n\r\n<ENTER> to continue...");

Console.ReadLine();
