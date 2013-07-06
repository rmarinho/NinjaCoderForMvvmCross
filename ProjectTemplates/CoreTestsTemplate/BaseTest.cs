﻿// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the BaseTest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross;
using Cirrious.MvvmCross.Platform;

namespace CoreTestsTemplate
{
    using Cirrious.CrossCore.Core;
    using Cirrious.MvvmCross.Test.Core;
    using Cirrious.MvvmCross.Views;

    using Mocks;

    using NUnit.Framework;

    /// <summary>
    /// Defines the BaseTest type.
    /// </summary>
    [TestFixture]
    public abstract class BaseTest : MvxIoCSupportingTest
    {
        /// <summary>
        /// The mock dispatcher.
        /// </summary>
        private MockDispatcher mockDispatcher;
        
        /// <summary>
        /// Sets up.
        /// </summary>
        [SetUp]
        public virtual void SetUp()
        {
            this.ClearAll();

            this.mockDispatcher = new MockDispatcher();

            Ioc.RegisterSingleton<IMvxViewDispatcher>(this.mockDispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadDispatcher>(this.mockDispatcher);
            Ioc.RegisterSingleton<IMvxTrace>(new TestTrace());
            Ioc.RegisterSingleton<IMvxSettings>(new MvxSettings());

            MvxSingletonCache.Initialise();

            MvxTrace.Initialize();

            this.Initialize();
            this.CreateTestableObject();
        }

        /// <summary>
        /// Initializes this instance.
        /// Any specific setup code for derived classes should override this method.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Terminates this instance.
        /// Any specific termination code for derived classes should override this method.
        /// </summary>
        public virtual void Terminate()
        {
        }

        /// <summary>
        /// Creates the testable object.
        /// </summary>
        public virtual void CreateTestableObject()
        {
        }

        /// <summary>
        /// Tears down.
        /// </summary>
        [TearDown]
        public virtual void TearDown()
        {
            this.Terminate();
        }
    }
}