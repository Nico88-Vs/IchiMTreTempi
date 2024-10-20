// Copyright QUANTOWER LLC. © 2017-2022. All rights reserved.

using System;
using System.Drawing;
using TradingPlatform.BusinessLayer;

namespace IchiMTreTempi
{
	public class IchiMTreTempi : Indicator
    {
        #region enum
        private enum Trend
        {
            Unknown,
            Up,
            Down,
        }

        private enum LineIndex
        {
            //moltiplicatore
            Tenkan_Sen = 0,
            Kijun_Sen = 1,
            Chikou_Span = 2,
            Senkou_SpanA = 3,
            Senkou_SpanB = 4,

            //moltiplicatore secondo
            Tenkan_Sen2 = 5,
            Kijun_Sen2 = 6,
            Chikou_Span2 = 7,
            Senkou_SpanA2 = 8,
            Senkou_SpanB2 = 9,

            //senza moltiplicatore
            Tenkan_Sen0 = 10,
            Kijun_Sen0 = 11,
            Chikou_Span0 = 12,
            Senkou_SpanA0 = 13,
            Senkou_SpanB0 = 14,

            LonGap = 16,
            ShortGap = 17,
            LonGap_Bigger = 18,
            ShortGap_Bigger = 19,
        }
        #endregion

        #region input
        //Setting Users imput
        [InputParameter("Tenkan Sen", 0, 1, 999, 1, 0)]     //nome,indice,valore.min,valore.max,precision
        public int TenkanPeriod = 9;

        [InputParameter("Kijoun Sen", 1, 1, 999, 1, 0)]
        public int KijounPeriod = 26;

        [InputParameter("SekuSpanB", 2, 1, 999, 1, 0)]
        public int SekuSpanBPeriod = 52;

        [InputParameter("CloudUp", 3)]
        public Color CloudUpColor = Color.FromArgb(50, Color.Green);

        [InputParameter("CloudUp2", 3)]
        public Color CloudUpColor2 = Color.FromArgb(50, Color.Green);

        [InputParameter("CloudUp0", 3)]
        public Color CloudUpColor0 = Color.FromArgb(50, Color.Green);

        [InputParameter("CloudDown", 3)]
        public Color CloudDownColor = Color.FromArgb(50, Color.Red);   // ATTENZIONE VERIFICARE FUNZIONAMENTO INDICE

        [InputParameter("CloudDown2", 3)]
        public Color CloudDownColor2 = Color.FromArgb(50, Color.Red);

        [InputParameter("CloudDown0", 3)]
        public Color CloudDownColor0 = Color.FromArgb(50, Color.Red);  

        [InputParameter("Multiplaier", 4, 1, 20, 0)]
        public int Moltiplicatore = 5;

        [InputParameter("MultiplaierSecondo", 4, 1, 60, 0)]
        public int Moltiplicatore2 = 30;
        #endregion

        // variabili globali
        private Trend currenTrend;
        private Trend currenTrend2;
        private Trend currenTrend0;

        public IchiMTreTempi()
            : base()
        {
            Name = "IchiMTreTempi V.1";
            Description = "Prima Versione";

            #region lineseries
            // Defines line on demand with particular parameters.
            AddLineSeries("Tenkan-sen-0", Color.Blue, 1, LineStyle.Solid);
            AddLineSeries("Kijun-sen-1", Color.Red, 1, LineStyle.Solid);
            AddLineSeries("Chikou-span-2", Color.Orange, 1, LineStyle.Solid);
            AddLineSeries("SenkunSpan-a-3", Color.Green, 1, LineStyle.Solid);
            AddLineSeries("SenkunSpan-b-4", Color.Red, 1, LineStyle.Solid);

            //second indicator
            AddLineSeries("Tenkan-sen2-5", Color.Blue, 1, LineStyle.Solid);
            AddLineSeries("Kijun-sen2-6", Color.Red, 1, LineStyle.Solid);
            AddLineSeries("Chikou-span2-7", Color.Orange, 1, LineStyle.Solid);
            AddLineSeries("SenkunSpan-a2-8", Color.Green, 1, LineStyle.Solid);
            AddLineSeries("SenkunSpan-b2-9", Color.Red, 1, LineStyle.Solid);

            //no multiplaier
            AddLineSeries("Tenkan-sen0-10", Color.Blue, 1, LineStyle.Solid);
            AddLineSeries("Kijun-sen0-11", Color.Red, 1, LineStyle.Solid);
            AddLineSeries("Chikou-span0-12", Color.Orange, 1, LineStyle.Solid);
            AddLineSeries("SenkunSpan-a0-13", Color.Green, 1, LineStyle.Solid);
            AddLineSeries("SenkunSpan-b0-14", Color.Red, 1, LineStyle.Solid);

            //detector
            AddLineSeries("Detector", Color.Red, 1, LineStyle.Dash);

            //Gaps
            AddLineSeries("LonGap-15", Color.Violet, 3, LineStyle.Solid);
            AddLineSeries("ShortGap-16", Color.DarkViolet, 3, LineStyle.Solid);

            AddLineSeries("LonGapBigger-17", Color.LightGoldenrodYellow, 3, LineStyle.Solid);
            AddLineSeries("ShortGap-18", Color.OrangeRed, 3, LineStyle.Solid);
            #endregion

            UpdateType = IndicatorUpdateType.OnBarClose;
            SeparateWindow = false;
        }

        protected override void OnInit()
        {
            //shift set
            // Shifts first time frame's line by the multiplaier in order to get the right x cordinate of the value
            this.LinesSeries[Convert.ToInt32(LineIndex.Chikou_Span)].TimeShift = -this.KijounPeriod * Moltiplicatore;
            this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanA)].TimeShift = this.KijounPeriod * Moltiplicatore;
            this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB)].TimeShift = this.KijounPeriod * Moltiplicatore;

            // Shifts second time frame's line by the multiplaier in order to get the right x cordinate of the value
            this.LinesSeries[Convert.ToInt32(LineIndex.Chikou_Span2)].TimeShift = -this.KijounPeriod * Moltiplicatore2;
            this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanA2)].TimeShift = this.KijounPeriod * Moltiplicatore2;
            this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB2)].TimeShift = this.KijounPeriod * Moltiplicatore2;

            this.LinesSeries[Convert.ToInt32(LineIndex.Chikou_Span0)].TimeShift = -this.KijounPeriod;
            this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanA0)].TimeShift = this.KijounPeriod;
            this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB0)].TimeShift = this.KijounPeriod;

            
        }

        protected override void OnUpdate(UpdateArgs args)
        {
            if (Count > Convert.ToInt32(LineIndex.Chikou_Span))
            {
                //SetValue(Close(), Convert.ToInt32(LineIndex.Chikou_Span));
            }

            ////tenkansen
            if (this.Count > SekuSpanBPeriod * Moltiplicatore)
            {
                double tenkan = GetAvarage(TenkanPeriod, Moltiplicatore);
                SetValue(tenkan, Convert.ToInt32(LineIndex.Tenkan_Sen));


                double kijoun = GetAvarage(KijounPeriod, Moltiplicatore);
                SetValue(kijoun, Convert.ToInt32(LineIndex.Kijun_Sen));

                double spana = (GetAvarage(TenkanPeriod, Moltiplicatore) + GetAvarage(KijounPeriod, Moltiplicatore)) / 2;
                SetValue(spana, Convert.ToInt32(LineIndex.Senkou_SpanA));

                double spanb = GetAvarage(SekuSpanBPeriod, Moltiplicatore);
                SetValue(spanb, Convert.ToInt32(LineIndex.Senkou_SpanB));

                //gestione della nuvola
                var newTrend = spana == spanb ? Trend.Unknown : spana > spanb ? Trend.Up : Trend.Down;

                if (this.currenTrend != newTrend)
                {
                    this.EndCloud(Convert.ToInt32(LineIndex.Senkou_SpanA), Convert.ToInt32(LineIndex.Senkou_SpanB), this.GetColorByTrend(this.currenTrend));
                    this.BeginCloud(Convert.ToInt32(LineIndex.Senkou_SpanA), Convert.ToInt32(LineIndex.Senkou_SpanB), this.GetColorByTrend(newTrend));
                }

                this.currenTrend = newTrend;
            }

            //seconde linee
            if (Moltiplicatore2 > 1 && this.Count > SekuSpanBPeriod * Moltiplicatore2)
            {
                //cikou_span
                SetValue(Close(), Convert.ToInt32(LineIndex.Chikou_Span2));

                //tenkansen
                double tenkan2 = GetAvarage(TenkanPeriod, Moltiplicatore2);
                SetValue(tenkan2, Convert.ToInt32(LineIndex.Tenkan_Sen2));

                //kijoun
                double kijoun2 = GetAvarage(KijounPeriod, Moltiplicatore2);
                SetValue(kijoun2, Convert.ToInt32(LineIndex.Kijun_Sen2));

                //spanA
                double spana2 = (GetAvarage(TenkanPeriod, Moltiplicatore2) + GetAvarage(KijounPeriod, Moltiplicatore2)) / 2;
                SetValue(spana2, Convert.ToInt32(LineIndex.Senkou_SpanA2));

                //spanB
                double spanb2 = GetAvarage(SekuSpanBPeriod, Moltiplicatore2);
                SetValue(spanb2, Convert.ToInt32(LineIndex.Senkou_SpanB2));

                //gestione della nuvola
                var newTrend2 = spana2 == spanb2 ? Trend.Unknown : spana2 > spanb2 ? Trend.Up : Trend.Down;

                if (this.currenTrend2 != newTrend2)
                {
                    this.EndCloud(Convert.ToInt32(LineIndex.Senkou_SpanA2), Convert.ToInt32(LineIndex.Senkou_SpanB2), this.GetColorByTrend2(this.currenTrend2));
                    this.BeginCloud(Convert.ToInt32(LineIndex.Senkou_SpanA2), Convert.ToInt32(LineIndex.Senkou_SpanB2), this.GetColorByTrend2(newTrend2));
                }

                this.currenTrend2 = newTrend2;
            }

            //linee 0
            if (this.Count > SekuSpanBPeriod)
            {
                //cikou_span
                SetValue(Close(), Convert.ToInt32(LineIndex.Chikou_Span0));

                //tenkansen
                double tenkan0 = GetAvarage(TenkanPeriod, 1);
                SetValue(tenkan0, Convert.ToInt32(LineIndex.Tenkan_Sen0));

                //kijoun
                double kijoun0 = GetAvarage(KijounPeriod, 1);
                SetValue(kijoun0, Convert.ToInt32(LineIndex.Kijun_Sen0));

                //spanA
                double spana0 = (GetAvarage(TenkanPeriod, 1) + GetAvarage(KijounPeriod, 1)) / 2;
                SetValue(spana0, Convert.ToInt32(LineIndex.Senkou_SpanA0));

                //spanB
                double spanb0 = GetAvarage(SekuSpanBPeriod, 1);
                SetValue(spanb0, Convert.ToInt32(LineIndex.Senkou_SpanB0));

                //gestione della nuvola
                var newTrend0 = spana0 == spanb0 ? Trend.Unknown : spana0 > spanb0 ? Trend.Up : Trend.Down;

                if (this.currenTrend0 != newTrend0)
                {
                    this.EndCloud(Convert.ToInt32(LineIndex.Senkou_SpanA0), Convert.ToInt32(LineIndex.Senkou_SpanB0), this.GetColorByTrend0(this.currenTrend0));
                    this.BeginCloud(Convert.ToInt32(LineIndex.Senkou_SpanA0), Convert.ToInt32(LineIndex.Senkou_SpanB0), this.GetColorByTrend0(newTrend0));
                }

                this.currenTrend0 = newTrend0;
            }

            //tryngto detect gaps
            //double spanB = this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB0)].GetValue();
            //double spanAPrimo = this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanA0)].GetValue(offset: 1);
            //double spanBPrimo = this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB0)].GetValue(offset: 1);
            //double spanA = this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanA0)].GetValue();
            //bool cloudChange;
            //if (spanBPrimo <= spanAPrimo)
            //{
            //    cloudChange = true;
            //}
            //else cloudChange = false;

            //double d = 0;

            //if (spanA<spanB && spanB<=spanAPrimo && !cloudChange)
            //{
            //    this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB0)].SetMarker(26, new IndicatorLineMarker(Color.Green, upperIcon: IndicatorLineMarkerIconType.UpArrow));
            //    d = 1;
            //}
            //SetValue(d, lineIndex: 15);

            //TODO: l indicatore e shiftato di uno
            //TODO: si puo aggiungere un moltiplicatore per l area di interesse
            double value = -1;
            double short_value = -1;
            double valuebigger = 100000;
            double short_valuebigger = 100000;

            bool detectShort = TryDetectShoertGaps(this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanA)], this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB)], this.Moltiplicatore);
            if (detectShort)
            {
                short_value = this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB)].GetValue(0);
                this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB)].SetMarker(26 * Moltiplicatore, new IndicatorLineMarker(Color.Red, upperIcon: IndicatorLineMarkerIconType.DownArrow));
            }

            bool detectTimeZero = TryDetectGaps(this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanA)], this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB)], this.Moltiplicatore);
            if (detectTimeZero)
            {
                value = this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB)].GetValue(0);
                this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB)].SetMarker(26*Moltiplicatore, new IndicatorLineMarker(Color.Green, bottomIcon: IndicatorLineMarkerIconType.UpArrow));
            }
            
            bool detectTimeZeroBigger = TryDetectShoertGaps(this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanA2)], this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB2)], this.Moltiplicatore2);
            if (detectTimeZeroBigger)
            {
                short_valuebigger = this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB)].GetValue(0);
                this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB)].SetMarker(26*Moltiplicatore, new IndicatorLineMarker(Color.DarkRed, upperIcon: IndicatorLineMarkerIconType.DownArrow));
            }
            
            bool detectTimeZeroBiggerLong = TryDetectGaps(this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanA2)], this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB2)], this.Moltiplicatore2);
            if (detectTimeZeroBiggerLong)
            {
                valuebigger = this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB)].GetValue(0);
                this.LinesSeries[Convert.ToInt32(LineIndex.Senkou_SpanB)].SetMarker(26*Moltiplicatore, new IndicatorLineMarker(Color.DarkGreen, bottomIcon: IndicatorLineMarkerIconType.UpArrow));
            }

            SetValue(value, 16);
            SetValue(short_value, 17);
            SetValue(valuebigger, 18);
            SetValue(short_valuebigger, 19);

        }

        public double GetAvarage(int period, int multiplaier)
        {
            double high = this.GetPrice(PriceType.High);
            double low = this.GetPrice(PriceType.Low);

            for (int i = 0; i < period * multiplaier; i++)
            {
                double price = this.GetPrice(PriceType.High, i);
                if (price > high) { high = price; }

                double priceLow = this.GetPrice(PriceType.Low, i);
                if (priceLow < low) { low = priceLow; }
            }

            double avarage = (high + low) / 2;

            return avarage;
        }

        private bool TryDetectGaps( LineSeries fastLine , LineSeries slowline , int tempo )
        {
            double fastCloud = fastLine.GetValue();
            double slowCloud = slowline.GetValue();
            double slowCloudOfset = slowline.GetValue(tempo);
            double fastCloudOfset = fastLine.GetValue(tempo);
            bool gap = false;

            bool isCloudChanged = slowCloudOfset < fastCloudOfset? true: false;

            if (slowCloudOfset != 0 && !isCloudChanged)
                if(slowCloud < fastCloudOfset)
                    gap = true;

            return gap;
            
        }

        private bool TryDetectShoertGaps(LineSeries fastLine, LineSeries slowline, int tempo)
        {
            double fastCloud = fastLine.GetValue();
            double slowCloud = slowline.GetValue();
            double slowCloudOfset = slowline.GetValue(tempo);
            double fastCloudOfset = fastLine.GetValue(tempo);
            bool gap = false;

            bool isCloudChanged = slowCloudOfset >= fastCloudOfset ? true : false;

            if (slowCloudOfset != 0 && !isCloudChanged)
                if (slowCloud > fastCloudOfset)
                    gap = true;

            return gap;
        }

        #region ColorTrend
        private Color GetColorByTrend(Trend trend) => trend switch
        {
            Trend.Up => this.CloudUpColor,
            Trend.Down => this.CloudDownColor,
            _ => Color.Empty
        };

        private Color GetColorByTrend2(Trend trend) => trend switch
        {
            Trend.Up => this.CloudUpColor2,
            Trend.Down => this.CloudDownColor2,
            _ => Color.Empty
        };

        private Color GetColorByTrend0(Trend trend) => trend switch
        {
            Trend.Up => this.CloudUpColor0,
            Trend.Down => this.CloudDownColor0,
            _ => Color.Empty
        };
        #endregion

        public override void OnPaintChart(PaintChartEventArgs args)
        {
            var gr = args.Graphics;
            Font f = new Font("Arial", 10);

            //try draw
            gr.DrawString("First multiplaier : x " + Moltiplicatore, f, Brushes.LightGreen, 10, 75);
            gr.DrawString("Second multiplaier : x " + Moltiplicatore2, f, Brushes.LightGreen, 10, 100);
        }
    }
}
