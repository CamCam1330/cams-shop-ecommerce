using Dapper;
using SV22T1020670.DomainModels;
using SV22T1020670.DataLayers;
using System.Data;

namespace SV22T1020670.DataLayers.SQLServer
{
    /// <summary>
    /// Các chức năng xử lý dữ liệu liên quan đến đơn hàng và nội dung của đơn hàng
    /// </summary>
    public class OrderDAL : BaseDAL
    {
        public OrderDAL(string connectionString) : base(connectionString)
        {
        }

        // ============================================================================================
        // PHẦN 1: TÌM KIẾM VÀ HIỂN THỊ DANH SÁCH ĐƠN HÀNG
        // ============================================================================================

        /// <summary>
        /// Tìm kiếm và hiển thị đơn hàng dưới dạng phân trang 
        /// </summary>
        public async Task<IEnumerable<Order>> ListAsync(int page = 1, int pageSize = 0, int status = 0, DateTime? fromTime = null, DateTime? toTime = null, string searchValue = "")
        {
            searchValue = $"%{searchValue}%";
            using var connection = await OpenConnectionAsync();
            var sql = @"with cte as
                        (
                            select  row_number() over(order by o.OrderTime desc) as RowNumber,
                                    o.*,
                                    c.CustomerName,
                                    c.ContactName as CustomerContactName,
                                    c.Address as CustomerAddress,
                                    c.Phone as CustomerPhone,
                                    c.Email as CustomerEmail,
                                    e.FullName as EmployeeName,
                                    s.ShipperName,
                                    s.Phone as ShipperPhone        
                            from    Orders as o
                                    left join Customers as c on o.CustomerID = c.CustomerID
                                    left join Employees as e on o.EmployeeID = e.EmployeeID
                                    left join Shippers as s on o.ShipperID = s.ShipperID
                            where   (@Status = 0 or o.Status = @Status)
                                and (@FromTime is null or o.OrderTime >= @FromTime)
                                and (@ToTime is null or o.OrderTime <= @ToTime)
                                and (c.CustomerName like @SearchValue or e.FullName like @SearchValue or s.ShipperName like @SearchValue)
                        )
                        select * from cte 
                        where (@PageSize = 0) or (RowNumber between (@Page - 1) * @PageSize + 1 and @Page * @PageSize)
                        order by RowNumber";
            var parameters = new
            {
                page,
                pageSize,
                status,
                fromTime,
                toTime,
                searchValue
            };
            return await connection.QueryAsync<Order>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
        }

        /// <summary>
        /// Đếm số lượng đơn hàng tìm được 
        /// </summary>
        public async Task<int> CountAsync(int status = 0, DateTime? fromTime = null, DateTime? toTime = null, string searchValue = "")
        {
            searchValue = $"%{searchValue}%";
            using var connection = await OpenConnectionAsync();
            var sql = @"select  count(*)        
                        from    Orders as o
                                left join Customers as c on o.CustomerID = c.CustomerID
                                left join Employees as e on o.EmployeeID = e.EmployeeID
                                left join Shippers as s on o.ShipperID = s.ShipperID
                        where   (@Status = 0 or o.Status = @Status)
                            and (@FromTime is null or o.OrderTime >= @FromTime)
                            and (@ToTime is null or o.OrderTime <= @ToTime)
                            and (c.CustomerName like @SearchValue or e.FullName like @SearchValue or s.ShipperName like @SearchValue)";
            var parameters = new
            {
                status,
                fromTime,
                toTime,
                SearchValue = searchValue ?? ""
            };
            return await connection.ExecuteScalarAsync<int>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
        }

        /// <summary>
        /// Lấy danh sách đơn hàng của một khách hàng cụ thể (Dùng cho trang Lịch sử mua hàng)
        /// </summary>
        public async Task<IEnumerable<Order>> ListByCustomerAsync(int customerID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"SELECT * FROM Orders 
                        WHERE CustomerID = @CustomerID 
                        ORDER BY OrderTime DESC";

            return await connection.QueryAsync<Order>(sql, new { CustomerID = customerID });
        }

        // ============================================================================================
        // PHẦN 2: LẤY THÔNG TIN ĐƠN HÀNG (GET ORDER)
        // ============================================================================================

        /// <summary>
        /// Lấy thông tin của đơn hàng dựa vào mã đơn hàng
        /// </summary>
        public async Task<Order?> GetAsync(int orderID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"select o.*,
                                c.CustomerName,
                                c.ContactName as CustomerContactName,
                                c.Address as CustomerAddress,
                                c.Phone as CustomerPhone,
                                c.Email as CustomerEmail,
                                e.FullName as EmployeeName,
                                s.ShipperName,
                                s.Phone as ShipperPhone        
                        from    Orders as o
                                left join Customers as c on o.CustomerID = c.CustomerID
                                left join Employees as e on o.EmployeeID = e.EmployeeID
                                left join Shippers as s on o.ShipperID = s.ShipperID
                        where   o.OrderID = @OrderID";
            var parameters = new { orderID };
            return await connection.QueryFirstOrDefaultAsync<Order>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
        }

        // ============================================================================================
        // PHẦN 3: THÊM, XÓA, SỬA ĐƠN HÀNG
        // ============================================================================================

        /// <summary>
        /// Thêm mới đơn hàng
        /// </summary>
        public async Task<int> AddAsync(Order data)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"insert into Orders(CustomerId, OrderTime, DeliveryProvince, DeliveryAddress, EmployeeID, Status)
                        values(@CustomerID, getdate(), @DeliveryProvince, @DeliveryAddress, @EmployeeID, @Status);
                        select @@identity";
            return await connection.ExecuteScalarAsync<int>(sql: sql, param: data, commandType: System.Data.CommandType.Text);
        }

        /// <summary>
        /// Cập nhật thông tin cơ bản của đơn hàng (không bao gồm trạng thái quy trình)
        /// </summary>
        public async Task<bool> UpdateAsync(Order data)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"update Orders
                        set CustomerID = @CustomerID,
                            OrderTime = @OrderTime,
                            DeliveryProvince = @DeliveryProvince,
                            DeliveryAddress = @DeliveryAddress,
                            EmployeeID = @EmployeeID,
                            AcceptTime = @AcceptTime,
                            ShipperID = @ShipperID,
                            ShippedTime = @ShippedTime,
                            FinishedTime = @FinishedTime,
                            Status = @Status
                        where OrderID = @OrderID";
            return (await connection.ExecuteAsync(sql: sql, param: data, commandType: System.Data.CommandType.Text)) > 0;
        }

        /// <summary>
        /// Xóa đơn hàng theo mã đơn hàng
        /// </summary>
        public async Task<bool> DeleteAsync(int orderID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"delete from OrderDetails where OrderID = @OrderID;
                        delete from Orders where OrderID = @OrderID";
            var parameters = new { orderID };
            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: System.Data.CommandType.Text)) > 0;
        }

        /// <summary>
        /// Lưu đơn hàng và chi tiết đơn hàng trong một transaction (Dùng khi lập đơn hàng mới)
        /// </summary>
        public async Task<int> SaveOrderWithDetailsAsync(Order order, IEnumerable<OrderDetail> orderDetails)
        {
            using var connection = await OpenConnectionAsync();
            using var transaction = connection.BeginTransaction();
            try
            {
                // Lưu đơn hàng
                var sqlOrder = @"insert into Orders(CustomerId, OrderTime, DeliveryProvince, DeliveryAddress, EmployeeID, Status)
                                values(@CustomerID, getdate(), @DeliveryProvince, @DeliveryAddress, @EmployeeID, @Status);
                                select @@identity";
                var orderID = await connection.ExecuteScalarAsync<int>(
                    sql: sqlOrder,
                    param: order,
                    transaction: transaction,
                    commandType: CommandType.Text);

                // Lưu từng chi tiết đơn hàng
                var sqlDetail = @"insert into OrderDetails(OrderID, ProductID, Quantity, SalePrice) 
                                 values(@OrderID, @ProductID, @Quantity, @SalePrice)";

                foreach (var detail in orderDetails)
                {
                    var detailParams = new
                    {
                        OrderID = orderID,
                        detail.ProductID,
                        detail.Quantity,
                        detail.SalePrice
                    };
                    await connection.ExecuteAsync(
                        sql: sqlDetail,
                        param: detailParams,
                        transaction: transaction,
                        commandType: CommandType.Text);
                }

                await transaction.CommitAsync();
                return orderID;
            }
            catch
            {
                try { await transaction.RollbackAsync(); } catch { }
                throw;
            }
        }

        // ============================================================================================
        // PHẦN 4: CHI TIẾT ĐƠN HÀNG (ORDER DETAILS)
        // ============================================================================================

        /// <summary>
        /// Hiển thị danh sách chi tiết đơn hàng
        /// </summary>
        public async Task<IEnumerable<OrderDetail>> ListDetailsAsync(int orderID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"select  od.*, p.ProductName, p.Photo, p.Unit
                        from    OrderDetails as od
                                join Products as p on od.ProductID = p.ProductID
                        where od.OrderID = @OrderID";
            var parameters = new { orderID };
            return await connection.QueryAsync<OrderDetail>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
        }

        /// <summary>
        /// Lấy thông tin 1 dòng chi tiết đơn hàng
        /// </summary>
        public async Task<OrderDetail?> GetDetailAsync(int orderID, int productID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"select  od.*, p.ProductName, p.Photo, p.Unit
                            from    OrderDetails as od
                                    join Products as p on od.ProductID = p.ProductID
                            where od.OrderID = @OrderID and od.ProductID = @ProductID";
            var parameters = new { orderID, productID };
            return await connection.QueryFirstOrDefaultAsync<OrderDetail>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
        }

        /// <summary>
        /// Lưu thông tin chi tiết đơn hàng (Thêm mới hoặc Cập nhật)
        /// </summary>
        public async Task<bool> SaveDetailAsync(int orderID, int productID, int quantity, decimal salePrice)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"if exists(select * from OrderDetails where OrderID = @OrderID and ProductID = @ProductID)
                                update OrderDetails 
                                set Quantity = @Quantity, SalePrice = @SalePrice 
                                where OrderID = @OrderID and ProductID = @ProductID
                            else
                                insert into OrderDetails(OrderID, ProductID, Quantity, SalePrice) 
                                values(@OrderID, @ProductID, @Quantity, @SalePrice)";
            var parameters = new
            {
                orderID,
                productID,
                quantity,
                salePrice
            };
            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: System.Data.CommandType.Text)) > 0;
        }

        /// <summary>
        /// Xóa thông tin chi tiết của đơn hàng 
        /// </summary>
        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"delete from OrderDetails where OrderID = @OrderID and ProductID = @ProductID";
            var parameters = new
            {
                orderID,
                productID
            };
            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: System.Data.CommandType.Text)) > 0;
        }

        // ============================================================================================
        // PHẦN 5: XỬ LÝ TRẠNG THÁI ĐƠN HÀNG (WORKFLOW)
        // ============================================================================================

        /// <summary>
        /// Duyệt chấp nhận đơn hàng
        /// </summary>
        public async Task<bool> AcceptOrderAsync(int orderID)
        {
            string sql = @"UPDATE Orders 
                           SET Status = @Status, AcceptTime = GETDATE() 
                           WHERE OrderID = @OrderID AND Status = @OldStatus";

            using var connection = await OpenConnectionAsync();
            var result = await connection.ExecuteAsync(sql, new
            {
                Status = Constants.ORDER_ACCEPTED,
                OrderID = orderID,
                OldStatus = Constants.ORDER_INIT // Chỉ duyệt được đơn đang ở trạng thái mới (Init)
            });
            return result > 0;
        }

        /// <summary>
        /// Xác nhận chuyển đơn hàng cho đơn vị vận chuyển
        /// </summary>
        public async Task<bool> ShipOrderAsync(int orderID, int shipperID)
        {
            string sql = @"UPDATE Orders 
                           SET Status = @Status, ShipperID = @ShipperID, ShippedTime = GETDATE() 
                           WHERE OrderID = @OrderID AND Status = @OldStatus";

            using var connection = await OpenConnectionAsync();
            var result = await connection.ExecuteAsync(sql, new
            {
                Status = Constants.ORDER_SHIPPING,
                ShipperID = shipperID,
                OrderID = orderID,
                OldStatus = Constants.ORDER_ACCEPTED // Chỉ chuyển hàng được đơn đã duyệt
            });
            return result > 0;
        }

        /// <summary>
        /// Ghi nhận hoàn tất đơn hàng
        /// </summary>
        public async Task<bool> FinishOrderAsync(int orderID)
        {
            string sql = @"UPDATE Orders 
                           SET Status = @Status, FinishedTime = GETDATE() 
                           WHERE OrderID = @OrderID AND Status = @OldStatus";

            using var connection = await OpenConnectionAsync();
            var result = await connection.ExecuteAsync(sql, new
            {
                Status = Constants.ORDER_FINISHED,
                OrderID = orderID,
                OldStatus = Constants.ORDER_SHIPPING // Chỉ hoàn tất được đơn đang giao
            });
            return result > 0;
        }

        /// <summary>
        /// Hủy đơn hàng
        /// </summary>
        public async Task<bool> CancelOrderAsync(int orderID)
        {
            string sql = @"UPDATE Orders 
                           SET Status = @Status, FinishedTime = GETDATE() 
                           WHERE OrderID = @OrderID AND Status NOT IN (@FinishedStatus)";

            using var connection = await OpenConnectionAsync();
            var result = await connection.ExecuteAsync(sql, new
            {
                Status = Constants.ORDER_CANCEL,
                OrderID = orderID,
                FinishedStatus = Constants.ORDER_FINISHED // Không được hủy đơn đã hoàn tất
            });
            return result > 0;
        }

        /// <summary>
        /// Từ chối đơn hàng
        /// </summary>
        public async Task<bool> RejectOrderAsync(int orderID)
        {
            string sql = @"UPDATE Orders 
                           SET Status = @Status, FinishedTime = GETDATE() 
                           WHERE OrderID = @OrderID AND Status = @OldStatus";

            using var connection = await OpenConnectionAsync();
            var result = await connection.ExecuteAsync(sql, new
            {
                Status = Constants.ORDER_REJECTED,
                OrderID = orderID,
                OldStatus = Constants.ORDER_INIT // Chỉ từ chối được đơn mới
            });
            return result > 0;
        }

        // ============================================================================================
        // PHẦN 6: LỊCH SỬ MUA HÀNG
        // ============================================================================================

        /// <summary>
        /// Kiểm tra xem khách hàng đã mua và hoàn tất đơn hàng có chứa sản phẩm này chưa
        /// </summary>
        public async Task<bool> HasPurchasedProductAsync(int customerID, int productID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"SELECT TOP 1 1 
                        FROM Orders o
                        JOIN OrderDetails od ON o.OrderID = od.OrderID
                        WHERE o.CustomerID = @CustomerID 
                          AND od.ProductID = @ProductID
                          AND o.Status = @FinishedStatus";

            var result = await connection.ExecuteScalarAsync<int?>(sql, new
            {
                CustomerID = customerID,
                ProductID = productID,
                FinishedStatus = Constants.ORDER_FINISHED 
            });

            return result != null;
        }
    }
}