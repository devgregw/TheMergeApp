﻿#region LICENSE

// Project MergeApi:  InvalidParamGroupException.cs (in Solution MergeApi)
// Created by Greg Whatley on 03/20/2017 at 6:44 PM.
// 
// The MIT License (MIT)
// 
// Copyright (c) 2017 Greg Whatley
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

#region USINGS

using System;
using MergeApi.Tools;

#endregion

namespace MergeApi.Exceptions {
    public sealed class InvalidParamGroupException : Exception {
        public InvalidParamGroupException(Type action, string pg, Exception inner = null)
            : base($"The parameter group '{pg}' is not valid for action '{action.FullName}'", inner) {
            Utilities.AssertCondition(action != null, action);
            Utilities.AssertCondition(!string.IsNullOrWhiteSpace(pg), pg);
            ActionType = action;
            ParamGroup = pg;
        }

        public Type ActionType { get; }

        public string ParamGroup { get; }
    }
}