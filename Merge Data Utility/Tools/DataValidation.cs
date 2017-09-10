#region LICENSE

// Project Merge Data Utility:  DataValidation.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 06/23/2017 at 10:45 AM.
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MergeApi.Client;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Framework.Interfaces;
using MergeApi.Models.Core;
using MergeApi.Models.Elements;
using MergeApi.Tools;
using Merge_Data_Utility.UI.Windows;

#endregion

namespace Merge_Data_Utility.Tools {
    public enum FixResult {
        Success,
        Error,
        Cancel
    }

    public static class DataValidation {
        public static async Task<FixResult> ApplyFix(ValidationResult v) {
            return await GetRepairFunc(v)(v);
        }

        private static Func<ValidationResult, Task<FixResult>> GetRepairFunc(ValidationResult v) {
            switch (v.ResultType) {
                case ValidationResultType.Success:
                    return DefaultSuccess;
                case ValidationResultType.Exception:
                    return DefaultError;
                //MAINS
                case ValidationResultType.OutdatedEvent:
                    var e = v.GetSubject<MergeEvent>();
                    var choice = GetUserChoice($"Fix events/{e.Id} - Data Validation",
                        v.ResultDescription,
                        new[] {"Change the dates", "Delete the event"});
                    if (choice == -1)
                        return DefaultCancel;
                    if (choice == 0)
#pragma warning disable 1998
                        return async v2 => {
                            var window = EditorWindow.Create(e, false, r => { });
                            window.ShowDialog();
                            return window.Result == EditorWindow.ResultType.Canceled
                                ? FixResult.Cancel
                                : FixResult.Success;
                        };
#pragma warning restore 1998
                    return GetDeleteFunc(v.GetSubject<MergeEvent>(),
                        v.GetSubject<MergeEvent>().Title, "event");
                case ValidationResultType.PageActionValidationFailure:
                    return GetPavfFixer(v);
                case ValidationResultType.PageContentValidationFailure:
                    return async v2 => {
                        var buttons = v.GetParameter<List<ElementBase>>().OfType<ButtonElement>().ToList();
                        var page = v.GetSubject<MergePage>();
                        foreach (var b in buttons) {
                            ValidationResult validation = await b.ValidateAsync(),
                                subvalidation = validation.GetParameter<ValidationResult>(),
                                subvalidation2 = subvalidation.GetParameter<ValidationResult>();
                            var choice1 = GetUserChoice($"Fix pages/{page.Id} - Data Validation",
                                $"Element {buttons.IndexOf(b) + 1} of {buttons.Count}: {validation.ResultDescription}",
                                new[] {
                                    "Reconfigure the action",
                                    "See more choices"
                                });
                            if (choice1 == 0) {
                                var copy = b;
                                var ci = page.Content.IndexOf(copy);
                                var window = new ActionConfigurationWindow(copy.Action);
                                window.ShowDialog();
                                if (window.Action == null) continue;
                                copy.Action = window.Action;
                                page.Content[ci] = copy;
                            } else if (choice1 == 1) {
                                await GetRepairFunc(subvalidation2)(subvalidation2);
                            } else {
                                await DefaultError(null);
                            }
                        }
                        try {
                            await MergeDatabase.UpdateAsync(page);
                            return FixResult.Success;
                        } catch {
                            return FixResult.Error;
                        }
                    };
                default:
#pragma warning disable 1998
                    return async v2 => {
                        MessageBox.Show("Uh oh", "Data Validation", MessageBoxButton.OK,
                            MessageBoxImage.Error, MessageBoxResult.OK);
                        return FixResult.Error;
                    };
#pragma warning restore 1998
            }
        }

        private static int GetUserChoice(string title, string message, string[] choices) {
            return ChoiceWindow.GetChoice(title, message, choices);
        }

        #region Specific Makers

        private static Func<ValidationResult, Task<FixResult>> GetPavfFixer(ValidationResult v) {
            var actionValidation = v.GetParameter<ValidationResult>();
            var page = v.GetSubject<MergePage>();
            switch (actionValidation.ResultType) {
                case ValidationResultType.Success:
                    return DefaultSuccess;
                case ValidationResultType.Exception:
                    return DefaultError;
                case ValidationResultType.EventNotFound:
                case ValidationResultType.PageNotFound:
                case ValidationResultType.GroupNotFound:
                    var c1 = GetUserChoice($"Fix pages/{page.Id} - Data Validation",
                        v.ResultDescription,
                        new[] {"Reconfigure the action", "Delete the page"});
                    if (c1 == -1)
                        return DefaultCancel;
                    if (c1 == 0)
                        return GetActionReconfigurationFunc(page);
                    return GetDeleteFunc(page,
                        page.Title, "page");
                case ValidationResultType.OutdatedAction:
                    var c4 = GetUserChoice($"Fix pages/{page.Id} - Data Validation",
                        v.ResultDescription,
                        new[] {"Reconfigure the action", "Delete the page"});
                    if (c4 == -1)
                        return DefaultCancel;
                    if (c4 == 0)
                        return GetActionReconfigurationFunc(page);
                    return GetDeleteFunc(page,
                        page.Title, "page");
                case ValidationResultType.NoAddress:
                    var c5 = GetUserChoice($"Fix pages/{page.Id} - Data Validation",
                        v.ResultDescription,
                        new[] {
                            "Reconfigure the action",
                            $"Specify an address for events/{actionValidation.GetParameter<MergeEvent>().Id}"
                        });
                    if (c5 == -1)
                        return DefaultCancel;
                    if (c5 == 0)
                        return GetActionReconfigurationFunc(page);
                    return async v2 => {
                        var input = TextInputWindow.GetInput("Specify An Address",
                            $"Enter the address for '{actionValidation.GetParameter<MergeEvent>().Location}':");
                        if (string.IsNullOrWhiteSpace(input))
                            return FixResult.Cancel;
                        var e = actionValidation.GetParameter<MergeEvent>();
                        e.Address = input;
                        try {
                            await MergeDatabase.UpdateAsync(e);
                            return FixResult.Success;
                        } catch {
                            return FixResult.Error;
                        }
                    };
                default:
                    var c2 = GetUserChoice($"Fix pages/{page.Id} - Data Validation",
                        v.ResultDescription,
                        new[] {
                            "Reconfigure the action",
                            $"Fix {actionValidation.ResultType.ToString().Replace("Action", "").Replace("ValidationFailure", "").ToLower()}s/{actionValidation.GetParameter<ValidationResult>().GetSubject<ModelBase>().Id}"
                        });
                    if (c2 == -1)
                        return DefaultCancel;
                    if (c2 == 0)
                        return GetActionReconfigurationFunc(page);
                    return GetRepairFunc(actionValidation.GetParameter<ValidationResult>());
            }
        }

        #endregion

        #region Generic Makers

        private static Func<ValidationResult, Task<FixResult>> GetDeleteFunc<T>(T obj, string title, string type)
            where T : IIdentifiable {
            return async v => {
                if (
                    MessageBox.Show($"Are you sure you want to delete the {type} '{title}' ({type + "s"}/{obj.Id})?",
                        "Data Validation: Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation,
                        MessageBoxResult.No) == MessageBoxResult.Yes)
                    try {
                        await MergeDatabase.DeleteAsync(obj);
                        return FixResult.Success;
                    } catch {
                        return FixResult.Error;
                    }
                return FixResult.Cancel;
            };
        }

        private static Func<ValidationResult, Task<FixResult>> GetActionReconfigurationFunc(MergePage p) {
            return async v => {
                var window = new ActionConfigurationWindow(p.ButtonAction);
                window.ShowDialog();
                if (window.Action == null)
                    return FixResult.Cancel;
                p.ButtonAction = window.Action;
                try {
                    await MergeDatabase.UpdateAsync(p);
                    return FixResult.Success;
                } catch {
                    return FixResult.Error;
                }
            };
        }

        #endregion

        #region Defaults

#pragma warning disable 1998

        private static readonly Func<ValidationResult, Task<FixResult>> DefaultSuccess = async v => FixResult.Success;

        private static readonly Func<ValidationResult, Task<FixResult>> DefaultError = async v => FixResult.Error;

        private static readonly Func<ValidationResult, Task<FixResult>> DefaultCancel = async v => FixResult.Cancel;

#pragma warning restore 1998

        #endregion
    }
}