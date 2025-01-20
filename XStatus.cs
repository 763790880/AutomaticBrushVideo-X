using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X学堂
{
    public class XStatus : INotifyPropertyChanged
    {
        protected string _Status;
        protected string _Schedule;
        protected string _Url;
        /// <summary>
        /// 网址
        /// </summary>
        [Display(Name ="网址")]
        public string Url
        {
            get => _Url;
            set
            {
                if (_Url != value)
                {
                    _Url = value;
                    OnPropertyChanged("Url");
                }
            }
        }
        /// <summary>
        /// 完成状态
        /// </summary>
        [Display(Name = "完成状态")]
        public string Status {
            get => _Status;
            set
            {
                if (_Status != value)
                {
                    _Status = value;
                    OnPropertyChanged("Status");
                }
            }
        }
        /// <summary>
        /// 进度
        /// </summary>
        [Display(Name = "进度")]
        public string Schedule {
            get => _Schedule;
            set
            {
                if (_Schedule != value)
                {
                    _Schedule = value;
                    OnPropertyChanged("Schedule");
                }
            }
        }
        /// <summary>
        /// Guid
        /// </summary>
        [Display(Name = "标识")]
        public string Guid { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
