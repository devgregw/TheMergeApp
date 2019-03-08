#region LICENSE

// Project MergeApi:  OpenAnnouncementDetailsAction.cs (in Solution MergeApi)
// Created by Greg Whatley on 06/23/2017 at 10:42 AM.
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

#endregion

namespace MergeApi.Models.Actions {
    /*
     * NOTICE: THIS OBJECT IS CURRENTLY PENDING DELETION
     * TODO: DELETE
     */

    /*public sealed class OpenAnnouncementDetailsAction : ActionBase {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "announcementId")]
        public string AnnouncementId1 { get; set; }

        public static OpenAnnouncementDetailsAction FromAnnouncementId(string aid) {
            Utilities.AssertCondition(!string.IsNullOrWhiteSpace(aid), aid);
            return new OpenAnnouncementDetailsAction {
                AnnouncementId1 = aid,
                ParamGroup = "1"
            };
        }

        public override void Invoke() {
            MergeDatabase.ActionInvocationReceiver.InvokeOpenAnnouncementDetailsActionAsync(this);
        }

        public override async Task<ValidationResult> ValidateAsync() {
            try {
                var resp = await MergeDatabase.GetAnnouncementAsync(AnnouncementId1);
                if (!resp.HasItem)
                    throw resp.Errors[0].Cause;
                var a = resp.Item;
                var v = await a.ValidateAsync();
                return v.ResultType != ValidationResultType.Success
                    ? new ValidationResult(this, ValidationResultType.AnnouncementValidationFailure, v)
                    : new ValidationResult(this);
            } catch (Exception ex) {
                if (!(ex is ApiResponseException))
                    return new ValidationResult(this, ValidationResultType.Exception, ex);
                var api = ex.Cast<Exception, ApiResponseException>();
                return api.Response.ResponseCode == 404
                    ? new ValidationResult(this, ValidationResultType.AnnouncementNotFound, AnnouncementId1)
                    : new ValidationResult(this, ValidationResultType.Exception, ex);
            }
        }

        public override string ToFriendlyString() {
            return $"Open annoouncement details: announcements/{AnnouncementId1}";
        }
    }*/
}