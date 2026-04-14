using SV22T1020670.DomainModels;
using SV22T1020670.DomainModels.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
namespace SV22T1020670.DomainModels
{
    public class OrderSearchInput : PaginationSearchInput
    {
        /// <summary>
        /// Trạng thái đơn hàng
        /// </summary>
            public int Status { get; set; } = 0;
        /// <summary>
        /// Khung thời gian
        /// </summary>
            public string DateRange { get; set; } = "";

            public class StatusOption
            {
                public int Value { get; set; }
                public string Text { get; set; } = "";
            }
        /// <summary>
        /// Danh sách trạng thái đơn hàng
        /// </summary>
            public List<StatusOption> StatusList
            {
                get
                {
                    return new List<StatusOption>
                {
                    new StatusOption { Value = 0, Text = "-- Trạng thái --" },
                    new StatusOption { Value = Constants.ORDER_INIT, Text = "Đơn hàng mới (chờ duyệt)" },
                    new StatusOption { Value = Constants.ORDER_ACCEPTED, Text = "Đơn hàng đã duyệt (chờ chuyển hàng)" },
                    new StatusOption { Value = Constants.ORDER_SHIPPING, Text = "Đơn hàng đang được giao" },
                    new StatusOption { Value = Constants.ORDER_FINISHED, Text = "Đơn hàng đã hoàn tất thành công" },
                    new StatusOption { Value = Constants.ORDER_CANCEL, Text = "Đơn hàng bị hủy" },
                    new StatusOption { Value = Constants.ORDER_REJECTED, Text = "Đơn hàng bị từ chối" }
                };
                }
            }

            /// <summary>
            /// Tự động tách chuỗi DateRange để lấy ngày bắt đầu
            /// </summary>
            public DateTime? FromDate
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DateRange)) return null;

                var parts = DateRange.Split('-');
                if (parts.Length >= 1)
                {
                    if (DateTime.TryParseExact(parts[0].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d))
                        return d;
                }
                return null;
            }
        }

        /// <summary>
        /// Tự động tách chuỗi DateRange để lấy ngày kết thúc
        /// </summary>
        public DateTime? ToDate
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DateRange)) return null;

                var parts = DateRange.Split('-');
                if (parts.Length >= 2)
                {
                    if (DateTime.TryParseExact(parts[1].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d))
                    {
                        // Quan trọng: Ngày kết thúc phải là cuối ngày (23:59:59.999) để tìm kiếm chính xác
                        return d.AddDays(1).AddTicks(-1);
                    }
                }
                return null;
            }
        }
    }
}