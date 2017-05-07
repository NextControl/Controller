using System;
using System.Collections.Generic;
using System.Text;

namespace ManiaNextControl.Classes
{
    public class HalfClass
    {
        public enum CurrentState
        {
            /// <summary>
            /// The fields of the class aren't set yet
            /// </summary>
            NotLoaded,
            /// <summary>
            /// Some of the fields are set
            /// </summary>
            PrimaryInfoFilled,
            /// <summary>
            /// All fields are set
            /// </summary>
            AllInfoFilled
        }

        /// <summary>
        /// Check the current loading state of the class
        /// </summary>
        public CurrentState LoadingState = CurrentState.NotLoaded;
    }
}
