﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Core.Commands
{
    public interface ICommand
    {
        bool IsValid { get; }
        IEnumerable<string> ValidationErrorMessges { get; }

        void Validate();
    }
}
