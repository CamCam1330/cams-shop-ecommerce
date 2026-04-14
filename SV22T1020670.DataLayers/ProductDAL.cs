using Dapper;
using SV22T1020670.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.DataLayers
{
    public class ProductDAL : BaseDAL
    {
        public ProductDAL(string connectionString) : base(connectionString)
        {
        }

        // --- XỬ LÝ SẢN PHẨM ---

        public async Task<IEnumerable<Product>> ListAsync(
            int page = 1,
            int pageSize = 0,
            string searchValue = "",
            int categoryID = 0,
            int supplierID = 0,
            decimal minPrice = 0,
            decimal maxPrice = 0,
            string sortBy = "")
        {
            if (page <= 0) page = 1;
            if (pageSize < 0) pageSize = 0;

            string sortQuery = "ProductName";

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "price_asc":
                        sortQuery = "Price ASC";
                        break;
                    case "price_desc":
                        sortQuery = "Price DESC";
                        break;
                    default:
                        sortQuery = "ProductName";
                        break;
                }
            }

            string sql = $@"SELECT *
                            FROM (
                                SELECT *, ROW_NUMBER() OVER(ORDER BY {sortQuery}) AS RowNumber
                                FROM Products
                                WHERE (@SearchValue = N'' OR ProductName LIKE @SearchValue)
                                    AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                                    AND (@SupplierID = 0 OR SupplierID = @SupplierID)
                                    AND (@MinPrice = 0 OR Price >= @MinPrice)
                                    AND (@MaxPrice = 0 OR Price <= @MaxPrice)
                            ) AS t
                            WHERE (@PageSize = 0) 
                                OR (RowNumber BETWEEN (@Page - 1) * @PageSize + 1 AND @Page * @PageSize)
                            ORDER BY RowNumber";

            using (var connection = OpenConnection())
            {
                var parameters = new
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchValue = $"%{searchValue}%",
                    CategoryID = categoryID,
                    SupplierID = supplierID,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };
                return await connection.QueryAsync<Product>(sql, parameters);
            }
        }

        public async Task<int> CountAsync(string searchValue = "", int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            string sql = @"SELECT COUNT(*) 
                           FROM Products 
                           WHERE (@SearchValue = N'' OR ProductName LIKE @SearchValue)
                             AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                             AND (@SupplierID = 0 OR SupplierID = @SupplierID)
                             AND (@MinPrice = 0 OR Price >= @MinPrice)
                             AND (@MaxPrice = 0 OR Price <= @MaxPrice)";

            using (var connection = OpenConnection())
            {
                var parameters = new
                {
                    SearchValue = $"%{searchValue}%",
                    CategoryID = categoryID,
                    SupplierID = supplierID,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };
                return await connection.ExecuteScalarAsync<int>(sql, parameters);
            }
        }

        public async Task<Product?> GetAsync(int productID)
        {
            string sql = @"SELECT * FROM Products WHERE ProductID = @ProductID";
            using (var connection = OpenConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { ProductID = productID });
            }
        }

        /// <summary>
        /// Thêm mới sản phẩm
        /// </summary>
        public async Task<int> AddAsync(Product data)
        {
            string sql = @"INSERT INTO Products(ProductName, ProductDescription, Unit, Price, SalePrice, Photo, CategoryID, SupplierID, IsSelling, Quantity)
                           VALUES(@ProductName, @ProductDescription, @Unit, @Price, @SalePrice, @Photo, @CategoryID, @SupplierID, @IsSelling, @Quantity);
                           SELECT @@IDENTITY;";
            using (var connection = OpenConnection())
            {
                return await connection.ExecuteScalarAsync<int>(sql, data);
            }
        }

        /// <summary>
        /// Cập nhật sản phẩm 
        /// </summary>
        public async Task<bool> UpdateAsync(Product data)
        {
            // Updated SQL to include Quantity
            string sql = @"UPDATE Products 
                           SET ProductName = @ProductName,
                               ProductDescription = @ProductDescription,
                               Unit = @Unit,
                               Price = @Price,
                               SalePrice = @SalePrice,
                               Photo = @Photo,
                               CategoryID = @CategoryID,
                               SupplierID = @SupplierID,
                               IsSelling = @IsSelling,
                               Quantity = @Quantity
                           WHERE ProductID = @ProductID";
            using (var connection = OpenConnection())
            {
                var result = await connection.ExecuteAsync(sql, data);
                return result > 0;
            }
        }

        public async Task<bool> DeleteAsync(int productID)
        {
            string sql = @"DELETE FROM Products WHERE ProductID = @ProductID";
            using (var connection = OpenConnection())
            {
                var result = await connection.ExecuteAsync(sql, new { ProductID = productID });
                return result > 0;
            }
        }

        public async Task<bool> InUsedAsync(int productID)
        {
            string sql = @"IF EXISTS(SELECT * FROM OrderDetails WHERE ProductID = @ProductID)
                                SELECT 1
                           ELSE 
                                SELECT 0";
            using (var connection = OpenConnection())
            {
                var result = await connection.ExecuteScalarAsync<int>(sql, new { ProductID = productID });
                return result > 0;
            }
        }

        /// <summary>
        /// Trừ số lượng tồn kho của sản phẩm
        /// </summary>
        public async Task<bool> DecreaseStockAsync(int productID, int quantityToMinus)
        {
            // Cập nhật: Số lượng mới = Số lượng cũ - Số lượng mua
            // Kèm điều kiện: Số lượng cũ phải >= Số lượng mua (để tránh âm kho)
            string sql = @"UPDATE Products 
                           SET Quantity = Quantity - @Quantity 
                           WHERE ProductID = @ProductID AND Quantity >= @Quantity";

            using (var connection = OpenConnection())
            {
                var result = await connection.ExecuteAsync(sql, new { ProductID = productID, Quantity = quantityToMinus });
                return result > 0; // Trả về True nếu trừ thành công, False nếu không đủ hàng
            }
        }

        // --- XỬ LÝ ẢNH (PHOTOS) ---
        public async Task<IEnumerable<ProductPhoto>> ListPhotosAsync(int productID)
        {
            string sql = @"SELECT * FROM ProductPhotos WHERE ProductID = @ProductID ORDER BY DisplayOrder";
            using (var connection = OpenConnection())
            {
                return await connection.QueryAsync<ProductPhoto>(sql, new { ProductID = productID });
            }
        }

        public async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            string sql = @"SELECT * FROM ProductPhotos WHERE PhotoID = @PhotoID";
            using (var connection = OpenConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<ProductPhoto>(sql, new { PhotoID = photoID });
            }
        }

        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            string sql = @"INSERT INTO ProductPhotos(ProductID, Photo, Description, DisplayOrder, IsHidden)
                           VALUES(@ProductID, @Photo, @Description, @DisplayOrder, @IsHidden);
                           SELECT @@IDENTITY;";
            using (var connection = OpenConnection())
            {
                return await connection.ExecuteScalarAsync<long>(sql, data);
            }
        }

        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            string sql = @"UPDATE ProductPhotos 
                           SET Photo = @Photo,
                               Description = @Description,
                               DisplayOrder = @DisplayOrder,
                               IsHidden = @IsHidden
                           WHERE PhotoID = @PhotoID";
            using (var connection = OpenConnection())
            {
                var result = await connection.ExecuteAsync(sql, data);
                return result > 0;
            }
        }

        public async Task<bool> DeletePhotoAsync(long photoID)
        {
            string sql = @"DELETE FROM ProductPhotos WHERE PhotoID = @PhotoID";
            using (var connection = OpenConnection())
            {
                var result = await connection.ExecuteAsync(sql, new { PhotoID = photoID });
                return result > 0;
            }
        }

        // --- XỬ LÝ THUỘC TÍNH (ATTRIBUTES) ---

        public async Task<IEnumerable<ProductAttribute>> ListAttributesAsync(int productID)
        {
            string sql = @"SELECT * FROM ProductAttributes WHERE ProductID = @ProductID ORDER BY DisplayOrder";
            using (var connection = OpenConnection())
            {
                return await connection.QueryAsync<ProductAttribute>(sql, new { ProductID = productID });
            }
        }

        public async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            string sql = @"SELECT * FROM ProductAttributes WHERE AttributeID = @AttributeID";
            using (var connection = OpenConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<ProductAttribute>(sql, new { AttributeID = attributeID });
            }
        }

        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            string sql = @"INSERT INTO ProductAttributes(ProductID, AttributeName, AttributeValue, DisplayOrder)
                           VALUES(@ProductID, @AttributeName, @AttributeValue, @DisplayOrder);
                           SELECT @@IDENTITY;";
            using (var connection = OpenConnection())
            {
                return await connection.ExecuteScalarAsync<long>(sql, data);
            }
        }

        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            string sql = @"UPDATE ProductAttributes 
                           SET AttributeName = @AttributeName,
                               AttributeValue = @AttributeValue,
                               DisplayOrder = @DisplayOrder
                           WHERE AttributeID = @AttributeID";
            using (var connection = OpenConnection())
            {
                var result = await connection.ExecuteAsync(sql, data);
                return result > 0;
            }
        }

        public async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            string sql = @"DELETE FROM ProductAttributes WHERE AttributeID = @AttributeID";
            using (var connection = OpenConnection())
            {
                var result = await connection.ExecuteAsync(sql, new { AttributeID = attributeID });
                return result > 0;
            }
        }


        // =======================================================================
        //                        --- XỬ LÝ REVIEW (Admin) ---
        // =======================================================================

        /// <summary>
        /// Lấy danh sách Review cho trang Admin (kết hợp tìm kiếm theo tên SP hoặc tên Khách)
        /// </summary>
        public IList<ProductReview> ListReviews(int page, int pageSize, string searchValue, out int rowCount)
        {
            if (searchValue != "")
                searchValue = "%" + searchValue + "%";

            using (var connection = OpenConnection())
            {
                var sql = @"SELECT *
                    FROM (
                        SELECT r.*, 
                               p.ProductName,      -- Cần tên SP để Admin biết đang review món nào
                               c.CustomerName, 
                               ROW_NUMBER() OVER(ORDER BY r.ReviewTime DESC) AS RowNumber
                        FROM ProductReviews r
                        JOIN Products p ON r.ProductID = p.ProductID
                        JOIN Customers c ON r.CustomerID = c.CustomerID
                        WHERE (@SearchValue = N'' OR p.ProductName LIKE @SearchValue OR c.CustomerName LIKE @SearchValue)
                    ) AS t
                    WHERE t.RowNumber BETWEEN (@Page - 1) * @PageSize + 1 AND @Page * @PageSize";

                var countSql = @"SELECT COUNT(*)
                         FROM ProductReviews r
                         JOIN Products p ON r.ProductID = p.ProductID
                         JOIN Customers c ON r.CustomerID = c.CustomerID
                         WHERE (@SearchValue = N'' OR p.ProductName LIKE @SearchValue OR c.CustomerName LIKE @SearchValue)";

                var parameters = new
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchValue = searchValue ?? ""
                };

                rowCount = connection.ExecuteScalar<int>(countSql, parameters);
                return connection.Query<ProductReview>(sql, parameters).ToList();
            }
        }

        /// <summary>
        /// Cập nhật trạng thái ẩn/hiện của Review
        /// </summary>
        public void ToggleReviewStatus(int reviewID)
        {
            using (var connection = OpenConnection())
            {
                // Đảo ngược trạng thái IsHidden (0 -> 1, 1 -> 0)
                var sql = "UPDATE ProductReviews SET IsHidden = CASE WHEN IsHidden = 0 THEN 1 ELSE 0 END WHERE ReviewID = @ReviewID";
                connection.Execute(sql, new { ReviewID = reviewID });
            }
        }

        /// <summary>
        /// Xóa Review
        /// </summary>
        public void DeleteReview(int reviewID)
        {
            using (var connection = OpenConnection())
            {
                var sql = "DELETE FROM ProductReviews WHERE ReviewID = @ReviewID";
                connection.Execute(sql, new { ReviewID = reviewID });
            }
        }

        // =======================================================================
        //                        --- XỬ LÝ REVIEW (Shop) ---
        // =======================================================================

        /// <summary>
        /// Lấy danh sách đánh giá của sản phẩm (Chỉ lấy những review không bị ẩn)
        /// </summary>
        public async Task<IEnumerable<ProductReview>> ListReviewsAsync(int productID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"SELECT r.*, c.CustomerName
                        FROM ProductReviews r
                        JOIN Customers c ON r.CustomerID = c.CustomerID
                        WHERE r.ProductID = @ProductID AND r.IsHidden = 0
                        ORDER BY r.ReviewTime DESC";

            return await connection.QueryAsync<ProductReview>(sql, new { ProductID = productID });
        }

        /// <summary>
        /// Thêm đánh giá mới
        /// </summary>
        public async Task<int> AddReviewAsync(ProductReview data)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"INSERT INTO ProductReviews(ProductID, CustomerID, Rating, Comment, ReviewTime, IsHidden)
                        VALUES(@ProductID, @CustomerID, @Rating, @Comment, GETDATE(), 0);
                        SELECT @@IDENTITY";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }
    }
}