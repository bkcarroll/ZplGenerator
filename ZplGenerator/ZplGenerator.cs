using System.Text;

public class ZplGenerator
{
    public enum Color { Black, White }
    public enum Roundness { None, Level1, Level2, Level3, Level4, Level5, Level6, Level7, Level8 }
    public enum Orientation { Normal, Rotate90, Invert, Rotate270 }
    public enum Justification { Left, Right, Auto, Center }
    public enum BarcodeLabelPosition { Top, Bottom }
    public enum BarcodeMode { Default, UccCaseMode, AutomaticMode, UccEanMode }
    public enum TextDirection { LeftToRight, TopToBottom, RightToLeft }

    StringBuilder Document = new StringBuilder();
    int _dpi;
    bool _suppressCrLf;
    int _maxWidth;

    public ZplGenerator(bool suppressCrLf = true, int dpi = 203, int maxWidth = 710)
    {
        _suppressCrLf = suppressCrLf;
        _dpi = dpi;
        _maxWidth = maxWidth;
    }

    public ZplGenerator Raw(string value)
    {
        Append(value);
        return this;
    }

    public override string ToString()
    {
        return Document.ToString();
    }

    public ZplGenerator Start()
    {
        Append("^XA");
        return this;
    }

    public ZplGenerator ReversePrint()
    {
        Append("^FR");
        return this;
    }

    /// <summary>
    /// Sets the printer's default font and font height.  The width is set proportional to the height.
    /// </summary>
    /// <param name="height">Font height in dots.</param>
    /// <param name="font">0-9 or A-Z</param>
    /// <returns></returns>
    public ZplGenerator SetFont(int height, string font = "0")
    {
        Append($"^CF{font},{height}");
        return this;
    }

    /// <summary>
    /// Sets the printer's default font, font height, and font width.
    /// </summary>
    /// <param name="font">0-9 or A-Z</param>
    /// <param name="height">Font height in dots.</param>
    /// <param name="width">Font width in dots.</param>
    /// <returns></returns>
    public ZplGenerator SetFont(string font, int height, int width)
    {
        Append($"^CF{font},{height},{width}");
        return this;
    }

    public ZplGenerator SetPosition(int x, int y)
    {
        Append(GetStartField(x, y));
        return this;
    }

    public ZplGenerator EndField()
    {
        Append("^FS");
        return this;
    }

    public ZplGenerator DrawHorizontalLineAt(int x, int y, int width, int lineThickness)
    {
        Append($"{GetStartField(x, y)}{GetBoxField(width, 1, lineThickness, Color.Black, Roundness.None)}^FS");
        return this;
    }

    public ZplGenerator DrawVerticalLineAt(int x, int y, int height, int lineThickness)
    {
        Append($"{GetStartField(x, y)}{GetBoxField(1, height, lineThickness, Color.Black, Roundness.None)}^FS");
        return this;
    }

    public ZplGenerator DrawBoxAt(int x, int y, int width, int height, int lineThickness, Color color = Color.Black, Roundness roundness = Roundness.None)
    {
        Append($"{GetStartField(x, y)}{GetBoxField(width, height, lineThickness, color, roundness)}^FS");
        return this;
    }

    public ZplGenerator DrawBox(int width, int height, int lineThickness, Color color = Color.Black, Roundness roundness = Roundness.None)
    {
        Append(GetBoxField(width, height, lineThickness, color, roundness));
        return this;
    }

    public ZplGenerator WriteText(string value)
    {
        Append($"^FD{value}");
        return this;
    }

    public ZplGenerator WriteTextAt(int x, int y, string value, Orientation orientation = Orientation.Normal, Justification justification = Justification.Left)
    {
        var fb = string.Empty;
        value = value.Replace("_", "_5F").Replace(" ", "_20");
        switch (justification)
        {
            case Justification.Left:
                fb = ",0";
                break;
            case Justification.Right:
                fb = ",1";
                break;
            case Justification.Auto:
                fb = ",2";
                break;
            case Justification.Center:
                fb = $"^FB{_maxWidth},1,0,C,0";
                break;
        }

        string ao = string.Empty;
        switch (orientation)
        {
            case Orientation.Normal:
                ao = $"^AON";
                break;
            case Orientation.Rotate90:
                ao = $"^AOR";
                break;
            case Orientation.Invert:
                ao = $"^AOI";
                break;
            case Orientation.Rotate270:
                ao = $"^AOB";
                break;
            default:
                ao = $"^AON";
                break;
        }

        Append($"^FO{(justification == Justification.Center ? 0 : x)},{y}{fb}{ao}^FH^FD{value}^FS");
        return this;
    }



    public ZplGenerator WriteUnderLine(int x, int y, int value, int c1, int c2)
    {
        Append($"^FO{x},{y}^GB{value},{c1},{c2}^FS");
        return this;
    }
    public ZplGenerator Data(string value)
    {
        Append(GetDataField(value));
        return this;
    }

    public ZplGenerator End()
    {
        Append("~JSN");
        Append("^XZ");
        return this;
    }

    public ZplGenerator AddVoidMessage()
    {
        Append("^FO0,0^GD650,700,10,,R ^FS");
        Append("^ADN,60,40^FO300,70^FDV^FS");
        Append("^ADN,60,40^FO300,130^FDO^FS");
        Append("^ADN,60,40^FO300,190^FDI^FS");
        Append("^ADN,60,40^FO300,250^FDD^FS");
        Append("^ADN,60,40^FO300,430^FDV^FS");
        Append("^ADN,60,40^FO300,490^FDO^FS");
        Append("^ADN,60,40^FO300,550^FDI^FS");
        Append("^ADN,60,40^FO300,610^FDD^FS");
        return this;
    }


    /// <summary>
    /// Changes the default barcode format parameters
    /// </summary>
    /// <param name="moduleWidth">Module Width in dots (1-10)</param>
    /// <param name="ratio">Wide bar to narrow bar width ration (2.0 to 3.0 in 0.1 increments)</param>
    /// <param name="barCodeHeight">Bar code height in dots/inches</param>
    /// <returns></returns>
    public ZplGenerator BarCodeFormat(int moduleWidth, decimal ratio, int barHeight)
    {
        Append($"^BY{moduleWidth},{ratio},{barHeight}");
        return this;
    }

    public ZplGenerator BarCode(Orientation orientation, int height, bool printText, BarcodeLabelPosition textPosition, bool useUccCheckDigit, BarcodeMode mode, int x, int y, string value)
    {
        Append($"^FO{x},{y}^BC{GetOrientationValue(orientation)},{height},{(printText ? "Y" : "N")},{(textPosition == BarcodeLabelPosition.Top ? "Y" : "N")},{(useUccCheckDigit ? "Y" : "N")},{GetBarcodeModeValue(mode)}^FD{value}^FS");
        return this;
    }

    public ZplGenerator BarCode(int x, int y, string value)
    {
        Append($"^FO{x},{y}^BC^FD{value}^FS");
        return this;
    }

    public int GetDotsFromInches(decimal inches)
    {
        return Convert.ToInt32(inches * Convert.ToDecimal(_dpi));
    }

    public ZplGenerator AddQrCodeAt(int x, int y, string data)
    {
        Append($"^BY2,2,0{GetStartField(x, y)}^BQN,2,5^FDLA,{data}^FS");
        return this;
    }

    public ZplGenerator WriteTextAtWithWrap(int x, int y, string value, int wrap, int lineInterval, int max, out int endingY, Orientation orientation = Orientation.Normal, Justification justification = Justification.Left)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            endingY = y;
            return this;
        }

        if (value.Length <= wrap)
        {
            endingY = y;

            WriteTextAt(x, y, value, orientation, justification);

            return this;
        }

        var s = value.Split(' ');
        var length = 0;
        var i = 0;
        var s1 = string.Empty;

        while (i < s.Length)
        {
            length += s[i].Length + 1;
            if (length <= wrap)
            {
                s1 += s[i] + " ";
                i++;
            }
            else
            {
                WriteTextAt(x, y, s1, orientation, justification);
                length = 0;
                s1 = string.Empty;
                y += lineInterval;

                if (y > max)
                {
                    endingY = y - lineInterval;
                    return this;
                }
            }
        }

        WriteTextAt(x, y, s1, orientation, justification);
        endingY = y;

        return this;
    }

    private string GetStartField(int x, int y)
    {
        return $"^FO{x},{y}";
    }

    private string GetBoxField(int width, int height, int lineThickness, Color color, Roundness roundness)
    {
        return $"^GB{width},{height},{lineThickness},{GetColorValue(color)},{(int)roundness}";
    }

    private void Append(string data)
    {
        Document.Append(data);
        Document.Append(GetCrLf());
    }

    private string GetDataField(string value)
    {
        return $"^FD{value}";
    }

    private char GetBarcodeModeValue(BarcodeMode mode)
    {
        switch (mode)
        {
            case BarcodeMode.Default:
            default:
                return 'N';
            case BarcodeMode.UccCaseMode:
                return 'U';
            case BarcodeMode.AutomaticMode:
                return 'A';
            case BarcodeMode.UccEanMode:
                return 'D';
        }
    }

    private char GetOrientationValue(Orientation orientation)
    {
        switch (orientation)
        {
            case Orientation.Normal:
            default:
                return 'N';
            case Orientation.Rotate90:
                return 'R';
            case Orientation.Invert:
                return 'I';
            case Orientation.Rotate270:
                return 'B';
        }
    }

    private string GetColorValue(Color color)
    {
        switch (color)
        {
            case Color.White:
                return "W";
            case Color.Black:
            default:
                return "B";
        }
    }

    private string GetCrLf()
    {
        return _suppressCrLf ? string.Empty : Environment.NewLine;
    }
}
