﻿// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the LanguageFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace NinjaCoder.MvvmCross.Factories
{
    using System.Collections.Generic;

    using NinjaCoder.MvvmCross.Factories.Interfaces;

    /// <summary>
    ///  Defines the LanguageFactory type.
    /// </summary>
    public class LanguageFactory : ILanguageFactory
    {
        /// <summary>
        /// Gets the languages.
        /// </summary>
        public IEnumerable<string> Languages
        {
            get
            {
                return new List<string>
                       {
                           "Current Culture",
                           "US English",
                           "French"
                       };
            }
        }
    }
}
