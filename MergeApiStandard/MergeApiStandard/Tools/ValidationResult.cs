#region LICENSE

// Project MergeApi:  ValidationResult.cs (in Solution MergeApi)
// Created by Greg Whatley on 06/23/2017 at 10:43 AM.
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

using MergeApi.Framework.Enumerations;
using MergeApi.Framework.Interfaces;

#endregion

namespace MergeApi.Tools {
    public sealed class ValidationResult {
        private readonly object _resultParam;

        private readonly IValidatable _subject;

        public ValidationResult(IValidatable subject) {
            _subject = subject;
            ResultType = ValidationResultType.Success;
        }

        public ValidationResult(IValidatable subject, ValidationResultType result, object param = null) {
            _subject = subject;
            _resultParam = param;
            ResultType = result;
        }

        public ValidationResultType ResultType { get; }

        public string ResultDescription {
            get {
                var final = "";
                final += ResultType.GetDescription();
                if (_resultParam is ValidationResult)
                    final += $": {GetParameter<ValidationResult>().ResultDescription}";
                return final;
            }
        }

        public T GetParameter<T>() where T : class => _resultParam as T;

        public T GetSubject<T>() where T : IValidatable => (T)_subject;
    }
}