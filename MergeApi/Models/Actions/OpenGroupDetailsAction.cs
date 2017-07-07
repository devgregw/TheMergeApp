using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Database;
using MergeApi.Client;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Core;
using MergeApi.Tools;
using Newtonsoft.Json;

namespace MergeApi.Models.Actions {
    public sealed class OpenGroupDetailsAction : ActionBase {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "groupId")]
        public string GroupId1 { get; set; }

        public static OpenGroupDetailsAction FromGroupId(string id) {
            return new OpenGroupDetailsAction {
                ParamGroup = "1",
                GroupId1 = id
            };
        }

        public override async Task<ValidationResult> ValidateAsync() {
            try {
                //try to get the group..  if it doesn't exist, an exception will be thrown
                await MergeDatabase.GetAsync<MergeGroup>(GroupId1);
                //if we made it here, the group exists
                return new ValidationResult(this);
            } catch (FirebaseException) {
                return new ValidationResult(this, ValidationResultType.GroupNotFound, GroupId1);
            } catch (Exception ex) {
                return new ValidationResult(this, ValidationResultType.Exception, ex);
            }
        }

        public override string ToFriendlyString() {
            return $"Open group details: groups/{GroupId1}";
        }

        public override void Invoke() {
            MergeDatabase.ActionInvocationReceiver.InvokeOpenGroupDetailsActionAsync(this);
        }
    }
}
