
namespace EasyGas.Shared.Enums
{
    public enum StockTransactionType
    {
        /// <summary>
        /// general transaction
        /// </summary>
        Completed = 1,

        /// <summary>
        /// driver pickup from business entity to customer
        /// </summary>
        DriverPickupCreated,
        DriverPickupCompleted,
        DriverPickupCancelled,

        /// <summary>
        /// driver drop to business entity from customer
        /// </summary>
        DriverDropCreated,
        DriverDropCompleted,
        DriverDropCancelled,

        /// <summary>
        /// customer pickup order from business entity
        /// </summary>
        CustomerPickupCreated,
        CustomerPickupCompleted,
        CustomerPickupCancelled,

        /// <summary>
        /// customer drops to business entity; Eg: surrender
        /// </summary>
        CustomerDropCreated,
        CustomerDropCompleted,
        CustomerDropCancelled,

        /// <summary>
        /// driver delivered order to customer; can be refill order also
        /// </summary>
        CustomerOrderDelivered,

        /// <summary>
        /// Stock request from businessEntity to admin; virtual stock will be increased
        /// </summary>
        StockRequest,

        /// <summary>
        /// manual stock update by admin/businessEntity; current stock will be increased
        /// </summary>
        StockFill,

        /// <summary>
        /// asset entered plant zone
        /// </summary>
        PlantEntry,

        /// <summary>
        /// asset exited plant zone
        /// </summary>
        PlantExit,

        /// <summary>
        /// pickup from business entity for refill from plant
        /// </summary>
        DriverPickupForRefill,

        /// <summary>
        /// drop at business entity after refill from plant
        /// </summary>
        DriverDropAfterRefill,
    }

}
