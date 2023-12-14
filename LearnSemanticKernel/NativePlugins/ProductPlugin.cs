using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace LearnSemanticKernel.NativePlugins
{
    public class ProductPlugin
    {
        [KernelFunction, Description("Get the current date")]
        public string Today() => DateTime.Today.ToLongDateString();

        [KernelFunction, Description("Get the current time")]
        public string Now() => DateTime.Today.ToLongDateString();
    }
}
