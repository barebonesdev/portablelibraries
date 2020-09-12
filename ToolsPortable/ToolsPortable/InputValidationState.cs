using System;
using System.Collections.Generic;
using System.Text;

namespace ToolsPortable
{
    public class InputValidationState
    {
        public static readonly InputValidationState Valid = new InputValidationState();

        public static InputValidationState Invalid(string errorMessage)
        {
            return new InputValidationState()
            {
                ErrorMessage = errorMessage
            };
        }

        private InputValidationState() { }

        public string ErrorMessage { get; private set; }
    }
}
