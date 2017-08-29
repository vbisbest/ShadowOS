using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace core.ui
{
    public class ColorInfo
    {
        public ColorInfo()
        {
            this.fontSize = 14.5;
            this.fontWeight = FontWeights.Normal;
            this.fontStyle = FontStyles.Normal;
        }

        public Color clr;
        public string id;
        public int index;
        public FontFamily font;
        //public Style style;     // XAML style reference
        public Brush brForeground;
        public Color clrBackground=Colors.White;
        public Brush brBackground;

        public double fontSize { get; set; }
        public FontWeight fontWeight { get; set; }
        public FontStyle fontStyle { get; set; }
        public TextDecorationCollection textDecoration { get; set; }

    }
    public class CharColor
    {
        public char ch;                 // character code
        public short cindex;            // color index

        public CharColor(char ch, short index)
        {
            cindex = index;
            this.ch = ch;
        }
        public override string ToString()
        {
            return string.Format("{0} ci={1}", ch, cindex);
        }

        internal void SetCIndex(short p)
        {
            cindex = p;
        }
    }
    public class SyntaxRow
    {
        UITransactableList<CharColor> rd;
        public SyntaxState entryState;
        public int extra=1;       // CRLF = 2, LF=1

        public SyntaxRow(SyntaxState entryState,UITransactionManager tm)
        {
            rd = new UITransactableList<CharColor>(tm);
            this.entryState = entryState;
        }
        public UITransactableList<CharColor> RowData
        {
            get { return rd; }
        }
        public int Length
        {
            get { return rd.Count; }
        }
        public char this[int index]
        {
            get 
            {
                if (index >= rd.Count)
                    return (char)0;
                return rd[index].ch; 
            }
        }

        internal List<CharColor> BreakAt(int x)
        {
            List<CharColor> nl = new List<CharColor>();
            for (int ix = x; ix < Length; ix++)
                nl.Add(rd[ix]);
            rd.RemoveRange(x, Length - x);
            return nl;
        }

        internal void RemoveUpto(int pos)
        {
            rd.RemoveRange(0, pos);
        }

        internal void RemoveToEOL(int pos)
        {
            rd.RemoveRange(pos,rd.Count - pos);
        }

        internal void ExtractUpto(StringBuilder sb, int p)
        {
            int ix;
            for (ix = 0; ix < p; ix++)
                sb.Append(rd[ix].ch);
        }

        internal void ExtractToEOL(StringBuilder sb, int p)
        {
            int ix;
            for (ix = p; ix < rd.Count; ix++)
                sb.Append(rd[ix].ch);
        }

        internal void ExtractRange(StringBuilder sb, int p1, int p2)
        {
            int ix;
            for (ix = p1; ix < p2; ix++)
                sb.Append(rd[ix].ch);
        }
        public string Text
        {
            get
            {
                char[] ca = new char[Length];
                int ix;
                for (ix = 0; ix < Length; ix++)
                {
                    ca[ix] = rd[ix].ch;
                }
                return new string(ca);
            }
        }

        public void Insert(int x, CharColor cc)
        {
            if (x >= rd.Count)
                rd.Add(cc);
            else
                rd.Insert(x, cc);
        }

        public void Add(CharColor cc)
        {
            rd.Add(cc);
        }

        internal void AddRange(IEnumerable<CharColor> data)
        {
            rd.AddRange(data);
        }

        internal void RemoveAt(int x)
        {
            rd.RemoveAt(x);
        }

        internal void RemoveRange(int p1, int p2)
        {
            rd.RemoveRange(p1, p2);
        }

        internal void SetCIndex(int pos, int cindex)
        {
            // Dont transact color changes
            rd[pos].cindex = (short)cindex;
        }
    }
    public class SyntaxPattern
    {
        public string pattern;
        public string stateTrans;           // if this pattern marks a state transition this is the pattern for it
        public bool bAfter;                 // true means the state trans starts after match, not at beginning of it
        public Regex rgPattern;
    }
    public class SyntaxState
    {
        public string name;
        public bool bDefault;               // default (starting) state
        List<SyntaxPattern> pl = new List<SyntaxPattern>();
        List<SyntaxPattern> tl = new List<SyntaxPattern>();     // state transitions patterns

        public void AddPattern(string stateTrans, string regex, bool bAfter)
        {
            SyntaxPattern sp = new SyntaxPattern();
            sp.stateTrans = stateTrans;
            sp.pattern = regex.Trim();
            sp.bAfter = bAfter;
            sp.rgPattern = new Regex(regex.Trim(),/*RegexOptions.Compiled|*/RegexOptions.Multiline);
            if (string.IsNullOrEmpty(stateTrans))
                pl.Add(sp);
            else
                tl.Add(sp);
        }
        public List<SyntaxPattern> PatternList
        {
            get { return pl; }
        }
        public Match FindStateBreak(string text, ref int pos, out SyntaxPattern spResult,out bool bAfter)
        {
            Match match;
            Match bestMatch = null;
            int nearest = int.MaxValue;
            bAfter = false;
            spResult = null;
            foreach (SyntaxPattern sp in tl)
            {
                match = sp.rgPattern.Match(text, pos);
                if (match.Success)
                {
                    if (match.Index < nearest)
                    {
                        nearest = match.Index;
                        spResult = sp;
                        bestMatch = match;
                        bAfter = sp.bAfter;
                    }
                }
            }
            return bestMatch;
        }
    }

    public class SelectionSpace
    {
        public int x1;
        public int x2;
        public int y1;
        public int y2;

        public void StartSelection(int x, int y)
        {
            x1 = x;
            x2 = x;
            y1 = y;
            y2 = y;
        }
        public void UpdateSelection(int x,int y)
        {
            if(y<y1)
            {
                y1 = y;
                x1 = x;
            }
            else if(y==y1)
            {
                if(x<=x1)
                {
                    x1 = x;
                }
                else
                {
                    x2 = x;
                    y2 = y;
                }
            }
            else
            {
                x2 = x;
                y2 = y;
            }
        }

        public bool IsInSelection(int x, int y)
        {
            if (y < y1)
                return false;
            if (y == y1)
            {
                if (x < x1)
                    return false;
            }
            if(y==y2)
            {
                if (x >= x2)
                    return false;
            }
            if (y > y2)
                return false;
            return true;
        }
        public void Clear()
        {
            x1 = 0;
            y1 = 0;
            x2 = 0;
            y2 = 0;
        }
        public bool IsEmpty
        {
            get
            {
                return (y1 == y2 && x1 == x2);
            }
        }
        public bool IsSingleLine
        {
            get
            {
                return y1 == y2;
            }
        }
    }
    /// <summary>
    /// Interaction logic for SyntaxEditor.xaml
    /// </summary>
    public partial class SyntaxEditor : UserControl
    {
        WriteableBitmap wb;
        int cx;     // cell width
        int cy;     // cell height
        byte[][] glyphs;
        Typeface tf = new Typeface("Consolas");
        int[] backBuffer;
        int aw;     // actual control width
        int ah;     // actual control height
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        Random rnd = new Random();
        List<ColorInfo> cil = new List<ColorInfo>();
        List<SyntaxState> ssl = new List<SyntaxState>();
        Dictionary<string, ColorInfo> cimap = new Dictionary<string, ColorInfo>();
        private ColorInfo defaultColor = new ColorInfo();
        UITransactableList<SyntaxRow> rows;
        int curx = 0;   // caret position X
        int cury = 0;   // caret position Y
        int tabLength = 2;
        int scrollTopLine = 0; // Current vertical scroll position
        int scrollLeftCol = 0; // Current horizontal scroll position
        SelectionSpace ss = new SelectionSpace();
        Color selectionBackground = Colors.LightBlue;
        bool bFreezeSelectionUpdates = false;
        int maxColumn = 0;
        SyntaxState defaultState;
        bool windowHooked = false;
        UITransactionManager tm = new UITransactionManager();
        SelectionSpace searchSpace = new SelectionSpace();
        Color searchMatchedBackColor = SystemColors.HighlightColor;
        Color searchMatchedForeColor = Colors.White;
        int indexLengthPairColorIndex = 1;

        public SyntaxEditor()
        {
            IndexLengthPairs = new List<IndexLengthPairData>();
            rows = new UITransactableList<SyntaxRow>(tm);
            InitializeComponent();
            Focusable = true;
            initializeFontData();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            if (!windowHooked)
            {
                windowHooked = true;
                var window = Window.GetWindow(this);
                window.KeyDown += window_KeyDown;
                window.KeyUp += window_KeyUp;
                window.TextInput += window_TextInput;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.KeyDown -= window_KeyDown;
            window.KeyUp -= window_KeyUp;
            window.TextInput -= window_TextInput;
            windowHooked = false;
        }

        private void clearScreen()
        {
            if (backBuffer == null)
                return;
            int ix;
            for (ix = 0; ix < backBuffer.Length; ix++) 
                backBuffer[ix] = 0;
        }

        private void drawStringAt(int x, int y, string str)
        {
            int ix;
            for(ix=0; ix<str.Length; ix++ )
            {
                drawCharAt(x + ix, y, str[ix],Colors.Black,Colors.White);
            }
        }

        private void blit()
        {
            if (backBuffer != null)
            {
                Int32Rect rect = new Int32Rect(0, 0, aw, ah);
                wb.WritePixels(rect, backBuffer, aw * 4, 0);
            }
        }

        private void initializeFontData()
        {
            FormattedText text = new FormattedText("X",
                    new CultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    tf,
                    this.FontSize,
                    Brushes.Black);

            cx = (int)Math.Ceiling(text.Width);
            cy = (int)Math.Ceiling(text.Height);
            caret.Y2 = caret.Y1 + cy;
            glyphs = new byte[32768][];
        }

        public void MoveTo(int x,int y)
        {
            curx=x;
            cury=y;
            ensureCursorVisible();
            updateCursorPosition();
            if (bShiftPressed || bMouseDragging)
            {
                if (!bFreezeSelectionUpdates)
                {
                    ss.UpdateSelection(x, y);
                    updatePage();
                }
            }
            else if (!ss.IsEmpty)
            {
                ss.Clear();
                updatePage();
            }
        }

        byte[] getPixelsForChar(char ch)
        {
            if (ch >= glyphs.Length)
                return glyphs[0];
            if (glyphs[ch] != null)
                return glyphs[ch];
            FormattedText text = new FormattedText(string.Format("{0}", ch),
                    new CultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    tf,
                    this.FontSize,
                    Brushes.Black);

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawText(text, new Point(0, 0));
            drawingContext.Close();

            int[] pixels = new int[cx * cy * 4];
            var bmp = new RenderTargetBitmap(cx, cy, 0, 0, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            bmp.CopyPixels(pixels, cx * 4, 0);
            byte[] alphas = new byte[cx * cy * 4];
            int ix;
            for (ix = 0; ix < alphas.Length;ix++ )
            {
                alphas[ix] = (byte)(pixels[ix] >> 24);
            }
            glyphs[ch] = alphas;
            return alphas;
        }

        static int intFromColor(Color clr)
        {
            int cv = 0;
            cv |= clr.A;
            cv <<= 8;
            cv |= clr.R;
            cv <<= 8;
            cv |= clr.G;
            cv <<= 8;
            cv |= clr.B;
            return cv;
        }

        void drawCharAt(int x,int y,char ch,Color fore,Color back)
        {
            byte[] pixels=getPixelsForChar(ch);
            if (pixels == null)
                return;
            int iy, ix;
            int bx = x * cx;
            int by = y * cy;
            int rowbase;
            int stride = aw;
            if ((y+1)*cy >= ah)
                return;
            if ((x+1)*cx >= aw)
                return;
            int src;
            
            for (iy = 0; iy < cy; iy++) 
            {
                rowbase = (by + iy) * stride + bx;
                for (ix = 0; ix < cx; ix++) 
                {
                    src = pixels[(iy * cx + ix)];
                    backBuffer[rowbase + ix] = blend(fore, back, src);
                }
            }
        }

        private int blend(Color fore, Color back, int alpha)
        {
            if (alpha == 255)
                return intFromColor(fore);
            else if (alpha == 0)
                return intFromColor(back);
            int r = fore.R * alpha + back.R * (255 - alpha);
            int g = fore.G * alpha + back.G * (255 - alpha);
            int b = fore.B * alpha + back.B * (255 - alpha);
            int cv = 255;
            cv <<= 8;
            cv |= (r >> 8);
            cv <<= 8;
            cv |= (g >> 8);
            cv <<= 8;
            cv |= (b >> 8);
            return cv;
        }

        int ct = 0;
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            int iy;
            DateTime dt = DateTime.Now;
            clearScreen();
            for (iy = 0; iy < 45; iy++)
            {
                string msg = string.Format("Ask not what your country can do for you.. {0} - {1}", dt, ct);
                drawStringAt(1, 1+iy, msg);
                ct++;
            }
            blit();
        }

        /// <summary>
        /// Copies characters from the row data
        /// </summary>
        private void updatePage()
        {
            if (backBuffer == null || cx==0)
                return;
            clearScreen();
            int iy;
            int numrows = ah / cy;
            if (hscroll.Visibility == System.Windows.Visibility.Visible)
                numrows = (ah - 18) / cy;
            for (iy = 0; iy < numrows; iy++) 
            {
                if (scrollTopLine + iy >= rows.Count)
                    break;
                updateRow(scrollTopLine + iy, false);
            }
            blit();
        }

        private void updateRow(int row,bool bBlit)
        {
            if (row >= rows.Count || row < 0)
                return;
            int ix;
            int numcols = aw / cx;
            var ccl = rows[row].RowData;
            int charIndex;
            ColorInfo ci;
            if (vscroll.Visibility == System.Windows.Visibility.Visible)
                numcols = (aw - 18) / cx;
            for (ix = 0; ix < numcols; ix++)
            {
                charIndex = ix + scrollLeftCol;
                if (charIndex >= ccl.Count)
                {
                    // Not part of a global redraw..
                    if (bBlit)
                    {
                        drawCharAt(ix, row - scrollTopLine, ' ', Colors.White, Colors.White);
                        continue;
                    }
                    else
                        break;
                }

                ci=cil[ccl[charIndex].cindex];
                if(searchSpace.IsInSelection(charIndex,row))
                    drawCharAt(ix, row - scrollTopLine, ccl[charIndex].ch, searchMatchedForeColor, searchMatchedBackColor);
                else if(ss.IsInSelection(charIndex, row))
                    drawCharAt(ix, row - scrollTopLine, ccl[charIndex].ch, ci.clr, selectionBackground);
                else
                    drawCharAt(ix, row - scrollTopLine, ccl[charIndex].ch, ci.clr, ci.clrBackground);
            }
            if(bBlit)
                blit();
        }

        #region Language Definition
        public void ImportLanguageDef(string xmlFilename)
        {
            try
            {
                Type type = typeof(SyntaxEditor);
                string resourceName = type.Namespace + "." + xmlFilename;
                var info = type.Assembly.GetManifestResourceInfo(resourceName);
                if (info != null)
                {
                    using (Stream stream = type.Assembly.GetManifestResourceStream(resourceName))
                    {
                        using (XmlReader reader = XmlReader.Create(stream))
                        {
                            XElement doc = XElement.Load(reader);
                            importColors(doc);
                            importStates(doc);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void importStates(XElement root)
        {
            SyntaxState ss;
            string id;
            foreach (XElement state in root.Descendants("state"))
            {
                ss = new SyntaxState();
                id = state.Attribute("id").Value;
                ss.name = id;
                importPatterns(ss, state);
                ssl.Add(ss);
            }
        }

        private void importPatterns(SyntaxState ss, XElement stateElement)
        {
            string stateTrans;
            XAttribute ste;
            bool bAfter = false;
            XAttribute after;

            foreach (XElement ele in stateElement.Descendants())
            {
                if (ele.Name != "pattern")
                    continue;
                ste = ele.Attribute("tostate");
                if (ste != null)
                    stateTrans = ste.Value;
                else
                    stateTrans = string.Empty;

                after = ele.Attribute("after");
                if (after != null)
                    bAfter = after.Value == "1";
                else
                    bAfter = false;

                ss.AddPattern(stateTrans, ele.Value, bAfter);
            }
        }

        private void importColors(XElement root)
        {
            Color clr;
            ColorInfo ci;
            string id;
            XAttribute bga;
            foreach (XElement ele in root.Descendants("color"))
            {
                clr = parseColor(ele.Attribute("value").Value);
                id = ele.Attribute("id").Value;
                ci = new ColorInfo();
                ci.id = id;
                ci.clr = clr;
                ci.index = cil.Count;
                cil.Add(ci);
                cimap.Add(id, ci);

                ci.brForeground = new SolidColorBrush(clr);
                if (ele.Attribute("bold") != null)
                    ci.fontWeight = FontWeights.ExtraBold;
                if (ele.Attribute("italic") != null)
                    ci.fontStyle = FontStyles.Italic;
                if (ele.Attribute("underline") != null)
                    ci.textDecoration = TextDecorations.Underline;
                if (ele.Attribute("fontsize") != null)
                {
                    int size = 12;
                    if (int.TryParse(ele.Attribute("fontsize").Value, out size))
                        ci.fontSize = size;
                }

                bga = ele.Attribute("background");
                if (bga != null)
                {
                    Color back = parseColor(bga.Value);
                    ci.clrBackground = back;
                    ci.brBackground = new SolidColorBrush(back);
                    // ARG! WPF supports this, SL doesnt.
                    //style.Setters.Add(new Setter(Run.BackgroundProperty, ci.brBackground));
                }
            }
        }

        private Color parseColor(string p)
        {
            if (p[0] != '#')
                return Colors.Black;
            if (p.Length == 7)
            {
                int r = parseHex(p.Substring(1, 2));
                int g = parseHex(p.Substring(3, 2));
                int b = parseHex(p.Substring(5, 2));
                return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
            }
            return Colors.Black;
        }

        private int parseHex(string p)
        {
            int ix;
            char ch;
            int val = 0;
            for (ix = 0; ix < p.Length; ix++)
            {
                ch = char.ToUpper(p[ix]);
                val <<= 4;
                val |= (ch >= 'A' ? ch - 'A' + 10 : ch - '0');
            }
            return val;
        }
        
        public static readonly DependencyProperty SyntaxLanguageProperty = DependencyProperty.Register("SyntaxLanguage", typeof(string), typeof(SyntaxEditor),
            new PropertyMetadata(OnSyntaxLanguageChanged));

        public string SyntaxLanguage
        {
            get { return (string)GetValue(SyntaxLanguageProperty); }
            set { SetValue(SyntaxLanguageProperty, value); }
        }

        private static void OnSyntaxLanguageChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            SyntaxEditor highlighter = (SyntaxEditor)sender;

            string language = args.NewValue as string;
            string xmlDefinition;

            switch (language)
            {
                case "HTTPRequest":
                case "HTTPResponse":
                default:
                    xmlDefinition = "HTMLJavascript.xml";
                    break;
            }
            highlighter.ImportLanguageDef(xmlDefinition);
            highlighter.recolorize();
        }

        public static readonly DependencyProperty IsModifiedProperty = DependencyProperty.Register("IsModified", typeof(bool), typeof(SyntaxEditor), new PropertyMetadata(false));

        public bool IsModified
        {
            get { return (bool)GetValue(IsModifiedProperty); }
            set 
            {
                if (value != IsModified)
                {
                    SetValue(IsModifiedProperty, value);
                }
            }
        }

        public static readonly DependencyProperty IsReadyOnlyProperty = DependencyProperty.Register("IsReadyOnly", typeof(bool), typeof(SyntaxEditor), new PropertyMetadata(false));

        public bool IsReadyOnly
        {
            get { return (bool)GetValue(IsReadyOnlyProperty); }
            set
            {
                SetValue(IsReadyOnlyProperty, value);
            }
        }

        #endregion

        private List<IndexLengthPairData> _indexLengthPairs;
        public List<IndexLengthPairData> IndexLengthPairs
        {
            get
            {
                return _indexLengthPairs;
            }
            set
            {
                _indexLengthPairs = value;
                recolorize();
            }
        }


        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SyntaxEditor),
            new PropertyMetadata(OnTextChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            SyntaxEditor highlighter = (SyntaxEditor)sender;

            string textData = args.NewValue as string;

            if (textData != null)
            {
               //highlighter.Text = textData;
            }

            if (!string.IsNullOrEmpty(highlighter.SyntaxLanguage))
                highlighter.recolorize();
            highlighter.tm.Clear();
        }

        public string SearchText
        {
            get
            {
                return search.Text;
            }
            set
            {
                search.Text = value;
                searchSpace.Clear();
                updatePage();
            }
        }

        private void recolorize()
        {
            if (rows != null)
            {
                tm.Enabled = false;
                rows.Clear();

                //DocumentWidth = 0;
                SyntaxState state;
                state = ssl.FirstOrDefault((a) => a.bDefault);
                // Assume first state is default if not otherwise specified
                if (state == null && ssl.Count > 0)
                    state = ssl[0];
                defaultState = state;
                SyntaxRow sr = new SyntaxRow(state, tm);
                rows.Add(sr);

                buildCCL(state);
                /*if (Text.HighlightSelection != null)
                {
                    foreach (IndexLength il in Text.HighlightSelection)
                    {
                        ForceColorIndex(il.Index, il.Index + il.Length, "AttackSelection");
                    }
                }*/
                //buildUIElements();
                syncVerticalScrollbar();
                recolorizeIndexLengthPairs();
                updateMaxColumn();
                updatePage();
                tm.Enabled = true;
            }
        }

        private void recolorizeIndexLengthPairs()
        {
            if (IndexLengthPairs != null)
            {
                foreach (var ilp in IndexLengthPairs)
                {
                    forceILPColor(ilp);
                }
            }
        }

        private void forceILPColor(IndexLengthPairData ilp)
        {
            int rowIndex;
            int sp = ilp.Index;
            
            for (rowIndex = 0; rowIndex < rows.Count; rowIndex++) 
            {
                if (sp < rows[rowIndex].Length + rows[rowIndex].extra)
                    break;
                sp -= rows[rowIndex].Length + rows[rowIndex].extra;
            }
            int col = sp;
            int runLength = ilp.Length;
            for (; rowIndex < rows.Count; rowIndex++)
            {
                var sr = rows[rowIndex];
                while(col<sr.Length)
                {
                    sr.SetCIndex(col, indexLengthPairColorIndex);
                    col++;
                    if (--runLength <= 0)
                        return;
                }
            }
        }
        /// <summary>
        /// This method breaks the text into sections by state.  Each of those
        /// sections are correspondingly broken into their appropriate colorized
        /// characters.
        /// </summary>
        /// <param name="state">Starting state</param>
        private void buildCCL(SyntaxState state)
        {
            int ix;
            SyntaxPattern sp;
            Match match;
            string statePart;
            int startPos;
            bool bAfter;
            
            if (Text == null)
                return;
            string trimmed = Text;
            SyntaxRow sr;
            
            for (ix = 0; ix < trimmed.Length; ix++)
            {
                if (state == null)
                {
                    switch(trimmed[ix])
                    {
                        case '\r' :
                            sr = new SyntaxRow(state,tm);
                            sr.extra = 2;
                            rows.Add(sr);
                            ix++;
                            break;
                        case '\n' :
                            sr = new SyntaxRow(state,tm);
                            rows.Add(sr);
                            break;
                        default :
                            rows[rows.Count - 1].Add(new CharColor(trimmed[ix], 0));
                            break;
                    }
                    continue;
                }
                startPos = ix;
                match = state.FindStateBreak(trimmed, ref ix, out sp, out bAfter);
                if (match == null)
                {
                    statePart = trimmed.Substring(startPos);
                    buildCCLInState(state, statePart);
                    return;
                }
                if(bAfter)
                    statePart = trimmed.Substring(startPos, match.Index + match.Length - startPos);
                else
                    statePart = trimmed.Substring(startPos, match.Index - startPos);
                buildCCLInState(state, statePart);
                // Transitioning to new state
                state = FindState(sp.stateTrans);
                if(bAfter)
                    ix = match.Index + match.Length - 1;
                else
                    ix = match.Index -1;
            }
        }

        private SyntaxState FindState(string p)
        {
            SyntaxState ss = ssl.FirstOrDefault((a) => a.name == p);
            if (ss == null)
                throw new Exception("State " + p + " referenced by not defined");
            return ss;
        }

        private void buildCCLInState(SyntaxState state, string statePart)
        {
            // Temporary list
            CharColor[] tca = new CharColor[statePart.Length];
            int ix;
            int groupIndex;
            for (ix = 0; ix < statePart.Length; ix++)
            {
                tca[ix] = new CharColor(statePart[ix], 0);
            }

            foreach (SyntaxPattern p in state.PatternList)
            {
                string[] sa = p.rgPattern.GetGroupNames();
                int[] cindices = new int[sa.Length];
                for (ix = 0; ix < sa.Length; ix++)
                {
                    var item = cil.FirstOrDefault(a => a.id == sa[ix]);
                    if (item != null)
                        cindices[ix] = cil.IndexOf(item);
                }
                MatchCollection mc = p.rgPattern.Matches(statePart);

                foreach (Match match in mc)
                {
                    if (match == null || match.Success == false)
                        continue;
                    for (groupIndex = 0; groupIndex < match.Groups.Count; groupIndex++)
                    {
                        // Skip unnamed groups
                        if (cindices[groupIndex] < 0)
                            continue;
                        Group group = match.Groups[groupIndex];
                        for (ix = 0; ix < group.Length; ix++)
                        {
                            tca[group.Index + ix].cindex = (short)cindices[groupIndex];
                        }
                    }
                }
            }
            appendTCA(state,tca);
            
        }

        private void appendTCA(SyntaxState state,CharColor[] tca)
        {
            int ix;
            int last = 0;
            int iy;
            for (ix = 0; ix < tca.Length;ix++ )
            {
                if (tca[ix].ch == '\n') 
                {
                    for (iy = last; iy < ix; iy++) 
                    {
                        if(tca[iy].ch!='\r')
                            rows[rows.Count - 1].Add(tca[iy]);
                    }
                    SyntaxRow sr = new SyntaxRow(state,tm);
                    rows.Add(sr);
                    last = ix + 1;
                }
            }
            for (iy = last; iy < ix; iy++)
            {
                if (tca[iy].ch != '\r')
                    rows[rows.Count - 1].Add(tca[iy]);
            }
        }

        string wordChars = " \t()[]=.+<>/";
        private bool isWordChar(char p)
        {
            return wordChars.IndexOf(p) >= 0;
        }

        private void cursorWordRight()
        {
            SyntaxRow sr = getRow(cury);
            if(sr==null)
                return;
            int x=curx;
            if(x>=sr.Length)
            {
                MoveTo(0, cury + 1);
                return;
            }
            while (x < sr.Length && !isWordChar(sr[x]))
                x++;
            while (x < sr.Length && isWordChar(sr[x]))
                x++;
            MoveTo(x, cury);
        }

        private void cursorWordLeft()
        {
            SyntaxRow sr = getRow(cury);
            if (sr == null)
                return;
            int x = curx;
            if (x <= 0)
            {
                if (cury > 0)
                {
                    sr = getRow(cury - 1);
                    MoveTo(sr.Length, cury - 1);
                }
                return;
            }
            if (x > 0)
                x--;
            while (x > 0 && isWordChar(sr[x]))
                x--;
            while (x>0 && !isWordChar(sr[x]))
                x--;
            MoveTo(x, cury);
        }

        private void cursorUp()
        {
            if (cury > 0)
            {
                MoveTo(curx, cury - 1);
                checkPassedEOL();
            }
        }

        private void cursorDown()
        {
            if (cury + 1 < rows.Count)
            {
                MoveTo(curx, cury + 1);
                checkPassedEOL();
            }
        }

        private void checkPassedEOL()
        {
            if (cury >= rows.Count)
            {
                MoveTo(curx, rows.Count - 1);
            }
            SyntaxRow sr = getRow(cury);
            if (sr == null)
                return;
            if (curx >= sr.Length)
            {
                MoveTo(sr.Length, cury);
            }
            
        }

        private void cursorHome()
        {
            MoveTo(0, cury);
        }

        private void cursorEOL()
        {
            SyntaxRow sr = getRow(cury);
            if (sr == null)
                MoveTo(0, cury);
            else
                MoveTo(sr.Length, cury);
        }

        private SyntaxRow getRow(int row)
        {
            if (row < 0 || row >= rows.Count)
                return null;
            return rows[row];
        }

        private void cursorRight()
        {
            SyntaxRow sr = getRow(cury);
            if (sr == null)
                return;
            if (curx >= sr.Length)
                MoveTo(0, cury + 1);
            else
                MoveTo(curx + 1, cury);
        }

        private bool cursorLeft()
        {
            if(curx<=0)
            {
                if(cury>0)
                {
                    SyntaxRow sr=getRow(cury-1);
                    MoveTo(sr.Length, cury - 1);
                    return true;
                }
                return false;
            }
            MoveTo(curx - 1, cury);
            return true;
        }

        void window_TextInput(object sender, TextCompositionEventArgs e)
        {             
            if (this.IsReadyOnly || !this.theimage.IsKeyboardFocused)
            {
                return;
            }
            insertText(e.Text,false);
            SyntaxRow sr = getRow(cury);
            if (sr != null)
            {
                RecolorizeLine(cury);
                if (sr.Length > maxColumn)
                    updateMaxColumn();
            }
            updateRow(cury, true);
        }

        private void insertText(string p, bool bDoUpdate)
        {
            if (IsReadyOnly)
            {
                return;
            }
            int ix;
            for (ix = 0; ix < p.Length; ix++)
            {
                switch (p[ix])
                {
                    case '\n':
                        RecolorizeLine(cury);
                        doReturn(p.Length == 1 && bDoUpdate);
                        break;
                    case '\r':
                    case '\v':
                    case '\b':
                        break;
                    default:
                        insertChar(p[ix], p.Length == 1 && bDoUpdate);
                        break;
                }
            }
            if (p.Length > 1 )
            {
                RecolorizeLine(cury);
                if (bDoUpdate)
                {
                    updatePage();
                    IsModified = true;
                }
            }
        }

        private void doTab()
        {
            int ix;
            for (ix = 0; ix < tabLength; ix++)
                insertChar(' ',true);
        }

        private void insertChar(char p,bool bDoUpdate)
        {
            if (IsReadyOnly)
            {
                return;
            }
            SyntaxRow sr = getRow(cury);
            if (sr == null)
            {
                SyntaxState ss;
                if (cury > 0)
                    ss = rows[cury - 1].entryState;
                else
                    ss = defaultState;
                sr = new SyntaxRow(ss,tm);
                rows.Add(sr);
                cursorEndOfDocument();
            }

            CharColor cc = new CharColor(p,0);
            sr.Insert(curx, cc);
            if (bDoUpdate)
                updateRow(cury, true);
            cursorRight();
            IsModified = true;
        }

        private void doReturn(bool bDoUpdate)
        {
            if (IsReadyOnly)
                return;
            SyntaxRow sr = getRow(cury);
            if (sr == null)
                return;
            SyntaxState ss = rows[cury].entryState;
            SyntaxRow nr = new SyntaxRow(ss,tm);
            insertRow(cury + 1, nr);
            if (curx < sr.Length)
            {
                var clip = sr.BreakAt(curx);
                nr.AddRange(clip);
            }
            MoveTo(0, cury + 1);
            if (bDoUpdate)
                updatePage();
            tm.Commit();
        }

        private void insertRow(int p, SyntaxRow nr)
        {
            if (IsReadyOnly)
            {
                return;
            }
            if (p >= rows.Count)
                rows.Add(nr);
            else
                rows.Insert(p, nr);
            syncVerticalScrollbar();
            IsModified = true;
        }

        private void backSpace()
        {
            if (IsReadyOnly)
            {
                return;
            }
            if (!ss.IsEmpty)
            {
                deleteSelection();
                return;
            }
            if (cursorLeft())
                deleteChar();
        }

        private void deleteChar()
        {
            if (IsReadyOnly)
            {
                return;
            }
            if (!ss.IsEmpty)
            {
                deleteSelection();
            }
            else
            {
                SyntaxRow sr = getRow(cury);
                if (sr == null)
                    return;
                if (curx < sr.Length)
                {
                    sr.RemoveAt(curx);
                    updateRow(cury, true);
                    IsModified = true;
                    return;
                }
                // Join row
                SyntaxRow next = getRow(cury + 1);
                if (next == null)
                    return;
                deleteRow(cury + 1);
                sr.AddRange(next.RowData.InternalList);
                updatePage();
                IsModified = true;
            }
        }

        private void deleteRow(int index)
        {
            if (IsReadyOnly)
            {
                return;
            }
            rows.RemoveAt(index);
            syncVerticalScrollbar();
            IsModified = true;
        }

        /// <summary>
        /// Adjusts the vertical scrollbar's properties based on the page size and content
        /// </summary>
        void syncVerticalScrollbar()
        {
            if (cy == 0)
                return;
            int pageSize = ah / cy;
            bool bShow = true;
            if (pageSize >= rows.Count)
            {
                vscroll.Maximum = 0;
                bShow = false;
            }
            else
                vscroll.Maximum = rows.Count-pageSize;
            if (vscroll.Value > vscroll.Maximum)
                vscroll.Value = vscroll.Maximum;
            vscroll.ViewportSize = pageSize;
            vscroll.SmallChange = 1;
            vscroll.LargeChange = pageSize;
            if (bShow)
            {
                thegrid.ColumnDefinitions[1].Width = new GridLength(18);
                vscroll.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                thegrid.ColumnDefinitions[1].Width = new GridLength(0);
                vscroll.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void vscroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scrollTopLine = (int)e.NewValue;
            updatePage();
            updateCursorPosition();
        }

        private void hscroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scrollLeftCol = (int)e.NewValue;
            updatePage();
            updateCursorPosition();
        }

        void updateMaxColumn()
        {
            int ix;
            int largestColumn = 0;
            for (ix = 0; ix < rows.Count; ix++)
            {
                largestColumn = Math.Max(rows[ix].Length, largestColumn);
            }
            if (largestColumn != maxColumn)
            {
                maxColumn = largestColumn;
                syncHorizontalScrollbar();
            }
        }

        private void syncHorizontalScrollbar()
        {
            if (cy == 0)
                return;
            int pageSize = aw / cx;
            if (pageSize >= maxColumn)
            {
                hscroll.Maximum = 0;
            }
            else
                hscroll.Maximum = maxColumn - pageSize;
            if (hscroll.Value > hscroll.Maximum)
                hscroll.Value = hscroll.Maximum;
            hscroll.ViewportSize = pageSize;
            hscroll.SmallChange = 1;
            hscroll.LargeChange = pageSize;
        }

        void updateCursorPosition()
        {
            caret.X1 = (curx-scrollLeftCol) * cx;
            caret.X2 = caret.X1;
            caret.Y1 = (cury-scrollTopLine) * cy;
            caret.Y2 = caret.Y1 + cy;
        }

        void ensureCursorVisible()
        {
            int pageSize = ah / cy;
            int pageWidth = aw / cx;
            if (vscroll.Visibility == System.Windows.Visibility.Visible)
                pageWidth = (aw - 18) / cx;
            if (hscroll.Visibility == System.Windows.Visibility.Visible)
                pageSize = (ah - 18) / cy;
            if (cury < scrollTopLine) 
            {
                vscroll.Value = cury;
            }
            if (cury - pageSize + 1 > scrollTopLine) 
            {
                vscroll.Value = cury - pageSize + 1;
            }

            if (curx < scrollLeftCol)
            {
                hscroll.Value = curx;
            }
            int pad = Math.Max(5,searchSpace.x2-searchSpace.x1);
            if (curx - pageWidth + pad > scrollLeftCol)
            {
                hscroll.Value = curx - pageWidth + pad;
            }
        }

        bool bCtrlPressed = false;
        bool bShiftPressed = false;

        void window_KeyUp(object sender, KeyEventArgs e)
        {
            if (!this.theimage.IsKeyboardFocused)
            {
                return;
            }
            switch (e.Key)
            {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    bCtrlPressed = false;
                    break;
                case Key.LeftShift :
                case Key.RightShift :
                    bShiftPressed = false;
                    break;
            }
        }

        void window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!this.theimage.IsKeyboardFocused)
            {
                return;
            }
            // control key states
            switch (e.Key)
            {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    bCtrlPressed = true;
                    break;
                case Key.LeftShift :
                case Key.RightShift :
                    bShiftPressed = true;
                    // If not continuing from a mouse selection
                    if(ss.IsEmpty)
                        ss.StartSelection(curx, cury);
                    break;
            }
            if (bCtrlPressed)
            {
                switch (e.Key)
                {
                    case Key.Right:
                        cursorWordRight();
                        break;
                    case Key.Left:
                        cursorWordLeft();
                        break;
                    case Key.Home :
                        MoveTo(0, 0);
                        break;
                    case Key.End :
                        cursorEndOfDocument();
                        break;
                    case Key.C :
                    case Key.Insert :
                        Copy();
                        break;
                    case Key.V :
                        Paste();
                        break;
                    case Key.X :
                        Cut();
                        break;
                    case Key.A:
                        SelectAll();
                        break;
                    case Key.Z :
                        undo();
                        break;
                    case Key.Y :
                        redo();
                        break;
                    case Key.F :
                        doFind();
                        break;
                    default:
                        // Prevent handled flag from setting
                        return;
                }
            }
            else if(bShiftPressed)
            {
                switch( e.Key )
                {
                    case Key.Delete :
                        Cut();
                        break;
                    case Key.Insert :
                        Paste();
                        break;

                    case Key.Left:
                        cursorLeft();
                        break;
                    case Key.Right:
                        cursorRight();
                        break;
                    case Key.Up:
                        cursorUp();
                        break;
                    case Key.Down:
                        cursorDown();
                        break;
                    case Key.End:
                        cursorEOL();
                        break;
                    case Key.Home:
                        cursorHome();
                        break;
                    case Key.PageUp:
                        pageUp();
                        break;
                    case Key.PageDown:
                        pageDown();
                        break;
                    default:
                        // Prevent handled flag from setting
                        return;
                }
            }
            // No control key
            else
            {
                switch (e.Key)
                {
                    case Key.Left:
                        cursorLeft();
                        break;
                    case Key.Right:
                        cursorRight();
                        break;
                    case Key.Up:
                        cursorUp();
                        break;
                    case Key.Down:
                        cursorDown();
                        break;
                    case Key.End:
                        cursorEOL();
                        break;
                    case Key.Home:
                        cursorHome();
                        break;
                    case Key.PageUp :
                        pageUp();
                        break;
                    case Key.PageDown:
                        pageDown();
                        break;

                    // Modification
                    case Key.Delete:
                        deleteChar();
                        break;
                    case Key.Back:
                        backSpace();
                        break;
                    case Key.Return:
                        doReturn(true);
                        break;
                    case Key.Tab:
                        doTab();
                        break;
                    case Key.F3 :
                        next_Click(sender, new RoutedEventArgs());
                        break;
                    default :
                        // Prevent handled flag from setting
                        return;
                }
            }
            e.Handled = true;
        }

        private void doFind()
        {
            findWindow.Visibility = System.Windows.Visibility.Visible;
            search.Text = "";
            search.Focus();
        }

        private void redo()
        {
            if (IsReadyOnly)
                return;
            tm.Redo();
            checkPassedEOL();
            updatePage();
        }

        private void undo()
        {
            if (IsReadyOnly)
                return;
            tm.Undo();
            checkPassedEOL();
            updatePage();
        }

        public void SelectAll()
        {
            if (rows.Count > 0)
            {
                SyntaxRow sr = rows[rows.Count - 1];
                ss.StartSelection(0, 0);
                ss.UpdateSelection(sr.Length, rows.Count - 1);
                updatePage();
            }
        }

        public void Paste()
        {
            if (IsReadyOnly)
            {
                return;
            }
            if (!Clipboard.ContainsText())
                return;
            try
            {
                if (!ss.IsEmpty)
                    deleteSelection();
                string text = Clipboard.GetText();
                bFreezeSelectionUpdates = true;
                insertText(text,true);
                ss.Clear();
                updateMaxColumn();
                updatePage();
                IsModified = true;
                tm.Commit();
            }
            finally
            {
                bFreezeSelectionUpdates = false;
            }
        }

        public void Copy()
        {
            string text = extractSelection();
            Clipboard.SetText(text);
        }

        private void pageUp()
        {
            if (cury <= 0)
                return;
            int pageSize = ah / cy;
            int ny = cury - pageSize;
            if (ny < 0)
                ny = 0;
            MoveTo(curx, ny);
            checkPassedEOL();
        }

        private void pageDown()
        {
            if (cury >= rows.Count)
                return;
            int pageSize = ah / cy;
            int ny = cury + pageSize;
            if (ny >= rows.Count)
                ny = rows.Count-1;
            MoveTo(curx, ny);
            checkPassedEOL();
        }

        private void cursorEndOfDocument()
        {
            if (rows.Count == 0)
                return;
            SyntaxRow sr = rows[rows.Count - 1];
            MoveTo(sr.Length, rows.Count - 1);
        }

        private void deleteSelection()
        {
            if (IsReadyOnly)
            {
                return;
            }
            SyntaxRow sr;
            if (ss.IsSingleLine)
            {
                sr = getRow(ss.y1);
                sr.RemoveRange(ss.x1, ss.x2-ss.x1);
                updateRow(ss.y1, false);
            }
            else
            {
                sr = getRow(ss.y1);
                sr.RemoveToEOL(ss.x1);
                SyntaxRow srbottom = getRow(ss.y2);
                srbottom.RemoveUpto(ss.x2);
                
                // Remove between layers
                int ix;
                for (ix = ss.y2 - 1; ix > ss.y1; ix--)
                    deleteRow(ix);

                if (srbottom.Length == 0)
                    rows.Remove(srbottom);
                if (sr.Length == 0)
                    rows.Remove(sr);
                // Merge these
                if(srbottom.Length!=0 && sr.Length!=0)
                {
                    rows.Remove(srbottom);
                    sr.AddRange(srbottom.RowData.InternalList);
                }
            }
            MoveTo(ss.x1, ss.y1);

            ss.Clear();
            updatePage();
            IsModified = true;
            tm.Commit();
        }

        public void Cut()
        {
            if (IsReadyOnly)
            {
                return;
            }
            if (!ss.IsEmpty)
            {
                string text = extractSelection();
                Clipboard.SetText(text);
                deleteSelection();
                IsModified = true;
            }
            else
            {
                if (cury < rows.Count)
                {
                    deleteRow(cury);
                    updatePage();
                    IsModified = true;
                }
            }
        }

        private string extractSelection()
        {
            SyntaxRow sr;
            StringBuilder sb = new StringBuilder();
            if (ss.IsSingleLine)
            {
                sr = getRow(ss.y1);
                sr.ExtractRange(sb, ss.x1, ss.x2);
                updateRow(ss.y1, false);
            }
            else
            {
                sr = getRow(ss.y1);
                sr.ExtractToEOL(sb, ss.x1);
                sb.AppendLine();

                int ix;
                for (ix = ss.y1+1; ix < ss.y2; ix++)
                {
                    rows[ix].ExtractToEOL(sb, 0);
                    sb.AppendLine();
                }

                SyntaxRow srbottom = getRow(ss.y2);
                srbottom.ExtractUpto(sb, ss.x2);
            }
            return sb.ToString();
        }

        bool bMouseDragging = false;
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Point pt = e.GetPosition(theimage);
                int x = (int)(pt.X / cx);
                int y = (int)(pt.Y / cy);
                x += scrollLeftCol;
                y += scrollTopLine;
                MoveTo(x, y);
                checkPassedEOL();
                bMouseDragging = true;
                ss.StartSelection(curx, cury);
            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            bMouseDragging = false;
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!bMouseDragging)
                return;
            if(e.LeftButton != MouseButtonState.Pressed)
            {
                bMouseDragging = false;
                return;
            }
            Point pt = e.GetPosition(theimage);
            int x = (int)(pt.X / cx);
            int y = (int)(pt.Y / cy);
            x += scrollLeftCol;
            y += scrollTopLine;
            MoveTo(x, y);
            checkPassedEOL();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (cx == 0)
                return;
            aw = (int)e.NewSize.Width;
            ah = (int)e.NewSize.Height;
            if (aw < 1)
                aw = 1;
            if (ah < 1)
                ah = 1;
            wb = new WriteableBitmap(aw, ah, 0, 0, PixelFormats.Pbgra32, null);
            backBuffer = new int[aw * ah];

            theimage.Source = wb;
            updatePage();

            syncVerticalScrollbar();
            syncHorizontalScrollbar();
            updateTextProperty();
            recolorize();
        }

        private void updateTextProperty()
        {
            if (!IsModified)
                return;
            StringBuilder sb = new StringBuilder();
            int ix;
            for (ix = 0; ix < rows.Count; ix++)
            {
                rows[ix].ExtractToEOL(sb, 0);
                if (ix + 1 < rows.Count)
                    sb.AppendLine();
            }
            Text = sb.ToString();
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int cv = (int)vscroll.Value;
            cv -= e.Delta/15;
            if (cv < 0)
                cv = 0;
            if (cv >= rows.Count)
                cv = rows.Count - 1;
            if(cv!=(int)vscroll.Value)
            {
                vscroll.Value = cv;
            }
        }

        public void RecolorizeLine(int row)
        {
            SyntaxRow sr = getRow(row);
            if (sr == null)
                return;
            SyntaxState state = sr.entryState;
            if (state == null)
                return;
            int ix;
            string text = sr.Text;
            int groupIndex;
            foreach (SyntaxPattern p in state.PatternList)
            {
                string[] sa = p.rgPattern.GetGroupNames();
                int[] cindices = new int[sa.Length];
                for (ix = 0; ix < sa.Length; ix++)
                {
                    var item = cil.FirstOrDefault(a => a.id == sa[ix]);
                    if (item != null)
                        cindices[ix] = cil.IndexOf(item);
                }
                MatchCollection mc = p.rgPattern.Matches(text);

                foreach (Match match in mc)
                {
                    if (match == null || match.Success == false)
                        continue;
                    for (groupIndex = 0; groupIndex < match.Groups.Count; groupIndex++)
                    {
                        // Skip unnamed groups
                        if (cindices[groupIndex] < 0)
                            continue;
                        Group group = match.Groups[groupIndex];
                        for (ix = 0; ix < group.Length; ix++)
                        {
                            sr.SetCIndex(group.Index + ix, cindices[groupIndex]);
                        }
                    }
                }
            }
        }

        private void theimage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            theimage.Focus();
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            searchSpace.Clear();
            findWindow.Visibility = System.Windows.Visibility.Collapsed;
            updatePage();
        }

        public bool FindNext()
        {
            int ix;
            int idx;
            string txt;
            
            for (ix = 0; ix < rows.Count; ix++)
            {
                int realIndex = (searchSpace.y1 + ix) % rows.Count;
                txt = rows[realIndex].Text;
                if (realIndex == searchSpace.y1 && searchSpace.x2 >= 0 && searchSpace.x2 < txt.Length)
                    idx = txt.IndexOf(search.Text, searchSpace.x2, StringComparison.InvariantCultureIgnoreCase);
                else
                    idx = txt.IndexOf(search.Text, StringComparison.InvariantCultureIgnoreCase);
                if (idx >= 0)
                {
                    searchSpace.x1 = idx;
                    searchSpace.y1 = realIndex;
                    searchSpace.y2 = realIndex;
                    searchSpace.x2 = idx + search.Text.Length;
                    MoveTo(idx, realIndex);
                    updatePage();
                    return true;
                }
            }

            return false;
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            FindNext();
        }

        private void search_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Return :
                    next_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                    break;
                case Key.F3:
                    next_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                    break;
            }
        }

        private void theimage_LostFocus(object sender, RoutedEventArgs e)
        {
            updateTextProperty();
        }
    }
}
