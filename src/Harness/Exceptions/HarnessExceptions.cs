using System;

namespace IEvangelist.Harness.Exceptions
{
    public class HarnessException : Exception
    {
        public HarnessException(string message) : base(message)
        {
        }
    }

    public class HarnessReadOnlyException : HarnessException
    {
        public HarnessReadOnlyException(string message) : base(message)
        {
        }
    }

    public class HarnessOptionNotFoundException : HarnessException
    {
        public HarnessOptionNotFoundException(string message) : base(message)
        {
        }
    }

    public class HarnessClassFailedException : HarnessException
    {
        public HarnessClassFailedException(string message) : base(message)
        {
        }
    }

    public class HarnessNoClassFailedException : HarnessException
    {
        public HarnessNoClassFailedException(string message) : base(message)
        {
        }
    }

    public class HarnessDisplayedFailedException : HarnessException
    {
        public HarnessDisplayedFailedException(string message) : base(message)
        {
        }
    }

    public class HarnessNotDisplayedFailedException : HarnessException
    {
        public HarnessNotDisplayedFailedException(string message) : base(message)
        {
        }
    }

    public class HarnessEnabledFailedException : HarnessException
    {
        public HarnessEnabledFailedException(string message) : base(message)
        {
        }
    }

    public class HarnessDisabledFailedException : HarnessException
    {
        public HarnessDisabledFailedException(string message) : base(message)
        {
        }
    }

    public class HarnessValueFailedException : HarnessException
    {
        public HarnessValueFailedException(string message) : base(message)
        {
        }
    }

    public class HarnessUrlFailedException : HarnessException
    {
        public HarnessUrlFailedException(string message) : base(message)
        {
        }
    }

    public class HarnessTitleFailedException : HarnessException
    {
        public HarnessTitleFailedException(string message) : base(message)
        {
        }
    }

    public class HarnessWriteFailedException : HarnessException
    {
        public HarnessWriteFailedException(string message) : base(message)
        {
        }
    }

    public class HarnessClickFailedException : HarnessException
    {
        public HarnessClickFailedException(string message) : base(message)
        {
        }
    }

    public class HarnessNoChildrenFoundException : HarnessException
    {
        public HarnessNoChildrenFoundException(string message) : base(message)
        {
        }
    }

    public class HarnessSendKeyFailedException : HarnessException
    {
        public HarnessSendKeyFailedException(string message) : base(message)
        {
        }
    }

    public class HarnessSwitchTabFailedException : HarnessException
    {
        public HarnessSwitchTabFailedException(string message) : base(message)
        {
        }
    }

    public class SelectElementFromDropDownFailedException : HarnessException
    {
        public SelectElementFromDropDownFailedException(string message) : base(message)
        {
        }
    }

    public class HoverElementFailedException : HarnessException
    {
        public HoverElementFailedException(string message) : base(message)
        {
        }
    }

    public class HarnessAttributeNotFound : HarnessException
    {
        public HarnessAttributeNotFound(string message) : base(message)
        {
        }
    }
}