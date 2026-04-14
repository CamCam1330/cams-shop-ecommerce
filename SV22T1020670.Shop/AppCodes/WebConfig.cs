namespace SV22T1020670.Shop.AppCodes
{
    public static class WebConfig
    {
        // Biến này sẽ nhận giá trị "https://localhost:44379" từ Program.cs
        public static string AdminServerUrl { get; set; } = "";

        public static string ProductImgPath { get; set; } = "/images/products/";
        public static string BannerImgPath { get; set; } = "/images/banners/";
        public static string CategoryImgPath { get; set; } = "/images/category/";

        /// <summary>
        /// Hàm sinh link ảnh (Trỏ thẳng sang Server Admin)
        /// </summary>
        public static string GetImageUrl(string type, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "/images/no-image.png";

            // Sử dụng AdminServerUrl đã đọc được từ Program.cs
            switch (type.ToLower())
            {
                case "products":
                    return $"{AdminServerUrl}{ProductImgPath}{fileName}";

                case "banners":
                    return $"{AdminServerUrl}{BannerImgPath}{fileName}";

                // 👇 THÊM ĐOẠN NÀY ĐỂ BẮT CẢ 2 TRƯỜNG HỢP 👇
                case "category":
                case "categories":
                    return $"{AdminServerUrl}{CategoryImgPath}{fileName}";

                default:
                    return $"{AdminServerUrl}/images/{fileName}";
            }
        }
    }
}