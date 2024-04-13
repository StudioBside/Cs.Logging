﻿namespace WikiTool.Core;

using Html2Markdown.Replacement;
using WikiTool.Core.Transform.CustomReplacer;

public sealed class PrePhaseRules : CompositeReplacer
{
    public PrePhaseRules()
    {
        // remove '<a href="#개요" class="toc-anchor">¶</a>'
        this.AddReplacer(new PatternReplacer
        {
            Pattern = @"<a href=""#.*?"" class=""toc-anchor"">¶</a>",
            Replacement = string.Empty,
        });

        this.AddReplacer(new ImgTagReplacer());
    }
}
