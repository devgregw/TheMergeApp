using System;

using UIKit;

namespace Merge.iOS.UI.ViewControllers {
    public partial class MainNavigationController : UINavigationController {
        public MainNavigationController() : base("MainNavigationController", null) {
        }

        public override void DidReceiveMemoryWarning() {
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad() {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view, typically from a nib.
        }
    }
}