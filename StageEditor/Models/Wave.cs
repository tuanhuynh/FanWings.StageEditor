using System.Collections.Generic;

namespace StageEditor.Models {

    public class Wave {
        public double Delay { get; set; }
        public double EndDelay { get; set; }
        public List<StageItem> Items { get; set; }
        public int StartNotification { get; set; }
        public int EndNotification { get; set; }

        public override string ToString() {
            string str = "Wave with ";
            if (Items.Count <= 0) {
                str += "no item";
            }else if (Items.Count == 1) {
                str += "1 item";
            } else {
                str +=  Items.Count +" items";
            }
            return str;
        }
    }

    public class Notification {
        public int Id { get; set; }
        public int Type { get; set; }
        public bool IsStart { get { return Type == 0; } }
        public string Content { get; set; }

        public override string ToString() {
            return Content;
        }
    }
}
