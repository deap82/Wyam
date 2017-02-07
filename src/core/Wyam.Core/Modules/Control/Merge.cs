﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Util;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Replaces the content and merges metadata of each input document with the results of specified modules.
    /// </summary>
    /// <remarks>
    /// Replaces the content and merges the metadata of each input document with the results of the specified modules
    /// executed against an empty initial document. If more than one output document is generated by the specified modules,
    /// each input document will be merged with each result document.
    /// </remarks>
    /// <category>Control</category>
    public class Merge : IModule
    {
        private readonly IModule[] _modules;
        private bool _forEachDocument;

        /// <summary>
        /// The specified modules are executed against an empty initial document and the results 
        /// are applied to every input document (possibly creating more than one output 
        /// document for each input document).
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public Merge(params IModule[] modules)
        {
            _modules = modules;
        }

        /// <summary>
        /// Specifies that the whole sequence of modules should be executed for every input document
        /// (as opposed to the default behavior of the sequence of modules only being executed once 
        /// with an empty initial document). This method has no effect if no modules are specified.
        /// </summary>
        public Merge ForEachDocument()
        {
            _forEachDocument = true;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            if (_modules != null && _modules.Length > 0)
            {
                // Execute the modules for each input document
                if (_forEachDocument)
                {
                    return inputs.SelectMany(context, input =>
                        context.Execute(_modules, new[] {input})
                            .Select(result => context.GetDocument(input, result.Content, result.Metadata))
                    );
                }

                // Execute the modules once and apply to each input document
                List<IDocument> results = context.Execute(_modules).ToList();
                return inputs.SelectMany(context, input =>
                    results.Select(result => context.GetDocument(input, result.Content, result.Metadata)));
            }

            return inputs;
        }
    }
}
