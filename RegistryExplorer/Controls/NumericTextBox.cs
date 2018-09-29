
using System.Windows.Forms;


namespace UI.Controls
{


    partial class NumericTextBox : TextBox
    {
        char decimalSeparator;
        char groupSeparator;
        char negativeSign;
        bool hexNumber;
        bool allowDecimal;
        bool allowGrouping;
        bool allowNegative;

        public override int MaxLength
        {
            get
            {
                return base.MaxLength;
            }
            set {}
        }

        public override bool ShortcutsEnabled
        {
            get
            {
                return base.ShortcutsEnabled;
            }
            set {}
        }

        public bool HexNumber
        {
            get { return hexNumber; }
            set
            {
                if (hexNumber ^ value)
                {
                    if (value)
                    {
                        if (AllowNegative)
                            Text = IntValue.ToString("x");
                        else
                            Text = UIntValue.ToString("x");
                        base.MaxLength = 8;
                    }
                    else
                    {
                        if (AllowNegative)
                            Text = IntValue.ToString();
                        else
                            Text = UIntValue.ToString();
                        base.MaxLength = 10;
                    }
                    hexNumber = value;
                }
            }
        }
        
        public bool AllowDecimal
        {
            get { return allowDecimal; }
            set
            {
                allowDecimal = value;
                FilterText();
            }
        }
        public bool AllowGrouping
        {
            get { return allowGrouping; }
            set
            {
                allowGrouping = value;
                FilterText();
            }
        }
        public bool AllowNegative 
        {
            get { return allowNegative; }
            set
            {
                allowNegative = value;
                FilterText();
            }
        }
        public int IntValue
        {
            get
            {
                try
                {
                    if (Text == string.Empty)
                        return 0;
                    else if (HexNumber)
                        return int.Parse(Text, System.Globalization.NumberStyles.HexNumber);
                    else
                        return int.Parse(Text);
                }
                catch
                {
                    return int.MaxValue;
                }                
            }
        }
        public uint UIntValue
        {
            get
            {
                try
                {
                    if (Text == string.Empty)
                        return 0;
                    else if (HexNumber)
                        return uint.Parse(Text, System.Globalization.NumberStyles.HexNumber);
                    else
                        return uint.Parse(Text);
                }
                catch (System.Exception)
                {
                    return uint.MaxValue;
                }
            }
        }

        public ulong ULongValue
        {
            get
            {
                try
                {
                    if (Text == string.Empty)
                        return 0;
                    else if (HexNumber)
                        return ulong.Parse(Text, System.Globalization.NumberStyles.HexNumber);
                    else
                        return ulong.Parse(Text);
                }
                catch (System.Exception)
                {
                    return ulong.MaxValue;
                }
            }
        }

        public float DecimalValue
        {
            get
            {
                try
                {
                    if (Text == string.Empty)
                        return 0;
                    else if (HexNumber)
                        return float.Parse(Text, System.Globalization.NumberStyles.HexNumber);
                    else
                        return float.Parse(Text);
                }
                catch
                {
                    return float.NaN;
                }
            }
        }

        public NumericTextBox()
        {
            InitializeComponent();
            System.Globalization.NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
            decimalSeparator = numberFormatInfo.NumberDecimalSeparator[0];
            groupSeparator = numberFormatInfo.NumberGroupSeparator[0];
            negativeSign = numberFormatInfo.NegativeSign[0];
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            e.Handled = !IsDigit(e.KeyChar);
        }

        private bool IsDigit(char ch)
        {            
            bool result = (
                            char.IsControl(ch) ||
                            char.IsDigit(ch) ||
                            (HexNumber && char.IsLetter(ch) && char.ToLower(ch) <= 'f') ||
                            (AllowNegative && Text.Length == 0 && ch.Equals(negativeSign)) ||
                            (AllowGrouping && Text.Length > 0 && ch.Equals(groupSeparator)) ||
                            (AllowDecimal && ch.Equals(decimalSeparator) && Text.IndexOf(decimalSeparator) == -1)
                           );
            return result;
        }
        
        private void FilterText()
        {
            System.Text.StringBuilder output = new System.Text.StringBuilder(Text.Length);
            foreach (char ch in Text)
            {
                if (IsDigit(ch))
                    output.Append(ch);
            }
            if (output.Length != Text.Length)
                Text = output.ToString();
        }


    }


}
