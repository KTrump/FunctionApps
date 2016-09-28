#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Indicator;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Strategy;
#endregion

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    /// <summary>
    /// Reference sample demonstrating how to achieve intrabar backtesting.
    /// </summary>
    [Description("Reference sample demonstrating how to achieve intrabar backtesting.")]
    public class SampleIntrabarBacktest : Strategy
    {
        #region Variables
		private int	fast	= 10;
		private int	slow	= 25;
        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
			/* Add a secondary bar series. 
			Very Important: This secondary bar series needs to be smaller than the primary bar series.
			
			Note: The primary bar series is whatever you choose for the strategy at startup. In this example I will
			reference the primary as a 5min bars series. */
			Add(PeriodType.Minute, 1);
			
			// Add two EMA indicators to be plotted on the primary bar series
			Add(EMA(Fast));
			Add(EMA(Slow));
			
			/* Adjust the color of the EMA plots.
			For more information on this please see this tip: http://www.ninjatrader-support.com/vb/showthread.php?t=3228 */
			EMA(Fast).Plots[0].Pen.Color = Color.Blue;
			EMA(Slow).Plots[0].Pen.Color = Color.Green;
			
            CalculateOnBarClose = true;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			/* When working with multiple bar series objects it is important to understand the sequential order in which the
			OnBarUpdate() method is triggered. The bars will always run with the primary first followed by the secondary and
			so on.
			
			Important: Primary bars will always execute before the secondary bar series.
			If a bar is timestamped as 12:00PM on the 5min bar series, the call order between the equally timestamped 12:00PM
			bar on the 1min bar series is like this:
				12:00PM 5min
				12:00PM 1min
				12:01PM 1min
				12:02PM 1min
				12:03PM 1min
				12:04PM 1min
				12:05PM 5min
				12:05PM 1min 
			
			When the OnBarUpdate() is called from the primary bar series (5min series in this example), do the following */
			if (BarsInProgress == 0)
			{
				// When the fast EMA crosses above the slow EMA, enter long on the secondary (1min) bar series
				if (CrossAbove(EMA(Fast), EMA(Slow), 1))
				{
					/* The entry condition is triggered on the primary bar series, but the order is sent and filled on the
					secondary bar series. The way the bar series is determined is by the first parameter: 0 = primary bars,
					1 = secondary bars, 2 = tertiary bars, etc. */
					EnterLong(1, 1, "Long: 1min");
				}
				
				// When the fast EMA crosses below the slow EMA, enter short on the secondary (1min) bar series
				else if (CrossBelow(EMA(Fast), EMA(Slow), 1))
				{
					/* The entry condition is triggered on the primary bar series, but the order is sent and filled on the
					secondary bar series. The way the bar series is determined is by the first parameter: 0 = primary bars,
					1 = secondary bars, 2 = tertiary bars, etc. */
					EnterShort(1, 1, "Short: 1min");
				}
			}
			
			// When the OnBarUpdate() is called from the secondary bar series, do nothing.
			else
			{
				return;
			}
        }

        #region Properties
		/// <summary>
		/// </summary>
		[Description("Period for fast MA")]
		[Category("Parameters")]
		public int Fast
		{
			get { return fast; }
			set { fast = Math.Max(1, value); }
		}

		/// <summary>
		/// </summary>
		[Description("Period for slow MA")]
		[Category("Parameters")]
		public int Slow
		{
			get { return slow; }
			set { slow = Math.Max(1, value); }
		}
        #endregion
    }
}
