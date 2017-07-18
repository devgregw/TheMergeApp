#region LICENSE

// Project MergeApi:  ValidationResultType.cs (in Solution MergeApi)
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

namespace MergeApi.Framework.Enumerations {
    public enum ValidationResultType {
        // <parameter>, <subject type(s) ([base type])>, [MAIN]
        Success, // nothing, anything

        EventNotFound, // id, AddToCalendarAction, GetDirectionsAction, OpenEventDetailsAction (ActionBase)

        //TODO: DELETE
        //AnnouncementNotFound, // id, ClickAnnouncementButtonAction, OpenAnnouncementDetailsAction (ActionBase)
        PageNotFound, // id, OpenPageAction
        GroupNotFound, // id, GetDirectionsAction, ShowContactInfoAction (ActionBase)
        LeaderNotFound, // id, ShowContactInfoAction

        //TODO: DELETE
        //ButtonHasNoAction, // MergeAnnouncement, ClickAnnouncementButtonAction
        //TODO: DELETE
        //AnnouncementValidationFailure, // ValidationResult, OpenAnnouncementDetailsAction
        PageValidationFailure, // ValidationResult, OpenPageAction
        EventValidationFailure, // ValidationResult, GetDirectionsAction, OpenEventDetailsAction (ActionBase)
        OutdatedAction, // AddToCalendarAction, AddToCalendarAction (same instance)

        ButtonActionValidationFailure, // ValidationResult, ButtonElement
        NoAddress, // MergeEvent, GetDirectionsAction

        Exception, // Exception, anything, MAIN
        PageActionValidationFailure, // ValidationResult, MergePage, MAIN

        //TODO: DELETE
        //AnnouncementActionValidationFailure, // ValidationResult, MergeAnnouncement, MAIN
        PageContentValidationFailure, // List<ElementBase>, MergePage, MAIN
        OutdatedEvent // MergeEvent, MergeEvent (same instance), MAIN
    }
}