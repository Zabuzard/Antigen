using System;

namespace Antigen.Statistics
{
    //TODO Documentation
    interface IStatisticsProvider
    {
        //TODO Documentation
        //TODO Check Resharper issue
// ReSharper disable EventNeverSubscribedTo.Global
        event EventHandler<StatisticsEventArgs> OnStatisticsUpdate;
// ReSharper restore EventNeverSubscribedTo.Global
    }
}
