#region Disclaimer / License

// Copyright (C) 2012, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

#endregion Disclaimer / License

using System;

namespace Maestro.Shared.UI
{
    /// <summary>
    /// Thrown when a lengthy operation is cancelled.
    /// </summary>
    public class CancelException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CancelException"/> class.
        /// </summary>
        public CancelException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CancelException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public CancelException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CancelException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public CancelException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}