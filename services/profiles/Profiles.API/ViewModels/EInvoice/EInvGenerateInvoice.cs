using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models.EInvoice
{
    public class EInvGenerateInvoiceRequest
    {
        public string access_token { get; set; }
        public string user_gstin { get; set; }
        public string data_source { get; set; }
        public TransactionDetails transaction_details { get; set; }
        public DocumentDetails document_details { get; set; }
        public SellerDetails seller_details { get; set; }
        public BuyerDetails buyer_details { get; set; }
        public DispatchDetails dispatch_details { get; set; }
        public ShipDetails ship_details { get; set; }
        public ExportDetails export_details { get; set; }
        public PaymentDetails payment_details { get; set; }
        public ReferenceDetails reference_details { get; set; }
        public List<AdditionalDocumentDetail> additional_document_details { get; set; }
        public ValueDetails value_details { get; set; }
        public EwaybillDetails ewaybill_details { get; set; }
        public List<ItemList> item_list { get; set; }
    }

    public class TransactionDetails
    {
        public string supply_type { get; set; }
        public string charge_type { get; set; }
        public string igst_on_intra { get; set; }
        public string ecommerce_gstin { get; set; }
    }

    public class DocumentDetails
    {
        public string document_type { get; set; }
        public string document_number { get; set; }
        public string document_date { get; set; }
    }

    public class SellerDetails
    {
        public string gstin { get; set; }
        public string legal_name { get; set; }
        public string trade_name { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string location { get; set; }
        public int pincode { get; set; }
        public string state_code { get; set; }
        public long phone_number { get; set; }
        public string email { get; set; }
    }

    public class BuyerDetails
    {
        public string gstin { get; set; }
        public string legal_name { get; set; }
        public string trade_name { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string location { get; set; }
        public int pincode { get; set; }
        public string place_of_supply { get; set; }
        public string state_code { get; set; }
        public long phone_number { get; set; }
        public string email { get; set; }
    }

    public class DispatchDetails
    {
        public string company_name { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string location { get; set; }
        public int pincode { get; set; }
        public string state_code { get; set; }
    }

    public class ShipDetails
    {
        public string gstin { get; set; }
        public string legal_name { get; set; }
        public string trade_name { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string location { get; set; }
        public int pincode { get; set; }
        public string state_code { get; set; }
    }

    public class ExportDetails
    {
        public string ship_bill_number { get; set; }
        public string ship_bill_date { get; set; }
        public string country_code { get; set; }
        public string foreign_currency { get; set; }
        public string refund_claim { get; set; }
        public string port_code { get; set; }
        public double export_duty { get; set; }
    }

    public class PaymentDetails
    {
        public string bank_account_number { get; set; }
        public int paid_balance_amount { get; set; }
        public int credit_days { get; set; }
        public string credit_transfer { get; set; }
        public string direct_debit { get; set; }
        public string branch_or_ifsc { get; set; }
        public string payment_mode { get; set; }
        public string payee_name { get; set; }
        public double outstanding_amount { get; set; }
        public string payment_instruction { get; set; }
        public string payment_term { get; set; }
    }

    public class DocumentPeriodDetails
    {
        public string invoice_period_start_date { get; set; }
        public string invoice_period_end_date { get; set; }
    }

    public class PrecedingDocumentDetail
    {
        public string reference_of_original_invoice { get; set; }
        public string preceding_invoice_date { get; set; }
        public string other_reference { get; set; }
    }

    public class ContractDetail
    {
        public string receipt_advice_number { get; set; }
        public string receipt_advice_date { get; set; }
        public string batch_reference_number { get; set; }
        public string contract_reference_number { get; set; }
        public string other_reference { get; set; }
        public string project_reference_number { get; set; }
        public string vendor_po_reference_number { get; set; }
        public string vendor_po_reference_date { get; set; }
    }

    public class ReferenceDetails
    {
        public string invoice_remarks { get; set; }
        public DocumentPeriodDetails document_period_details { get; set; }
        public List<PrecedingDocumentDetail> preceding_document_details { get; set; }
        public List<ContractDetail> contract_details { get; set; }
    }

    public class AdditionalDocumentDetail
    {
        public string supporting_document_url { get; set; }
        public string supporting_document { get; set; }
        public string additional_information { get; set; }
    }

    public class ValueDetails
    {
        public double total_assessable_value { get; set; }
        public double total_cgst_value { get; set; }
        public double total_sgst_value { get; set; }
        public double total_igst_value { get; set; }
        public double total_cess_value { get; set; }
        public double total_cess_value_of_state { get; set; }
        public double total_discount { get; set; }
        public double total_other_charge { get; set; }
        public double total_invoice_value { get; set; }
        public double round_off_amount { get; set; }
        public double total_invoice_value_additional_currency { get; set; }
    }

    public class EwaybillDetails
    {
        public string transporter_id { get; set; }
        public string transporter_name { get; set; }
        public string transportation_mode { get; set; }
        public string transportation_distance { get; set; }
        public string transporter_document_number { get; set; }
        public string transporter_document_date { get; set; }
        public string vehicle_number { get; set; }
        public string vehicle_type { get; set; }
    }

    public class BatchDetails
    {
        public string name { get; set; }
        public string expiry_date { get; set; }
        public string warranty_date { get; set; }
    }

    public class AttributeDetail
    {
        public string item_attribute_details { get; set; }
        public string item_attribute_value { get; set; }
    }

    public class ItemList
    {
        public int item_serial_number { get; set; }
        public string product_description { get; set; }
        public string is_service { get; set; }
        public string hsn_code { get; set; }
        public string bar_code { get; set; }
        public int quantity { get; set; }
        public int free_quantity { get; set; }
        public string unit { get; set; }
        public double unit_price { get; set; }
        public double total_amount { get; set; }
        public double pre_tax_value { get; set; }
        public double discount { get; set; }
        public double other_charge { get; set; }
        public double assessable_value { get; set; }
        public double gst_rate { get; set; }
        public double igst_amount { get; set; }
        public double cgst_amount { get; set; }
        public double sgst_amount { get; set; }
        public double cess_rate { get; set; }
        public double cess_amount { get; set; }
        public double cess_nonadvol_amount { get; set; }
        public double state_cess_rate { get; set; }
        public double state_cess_amount { get; set; }
        public double state_cess_nonadvol_amount { get; set; }
        public double total_item_value { get; set; }
        public string country_origin { get; set; }
        public string order_line_reference { get; set; }
        public string product_serial_number { get; set; }
        public BatchDetails batch_details { get; set; }
        public List<AttributeDetail> attribute_details { get; set; }
    }

    public class EInvGenerateInvoiceResponse
    {
        public Results results { get; set; }
    }

    public class Message
    {
        public long AckNo { get; set; }
        public string AckDt { get; set; }
        public string Irn { get; set; }
        public string SignedInvoice { get; set; }
        public string SignedQRCode { get; set; }
        public long EwbNo { get; set; }
        public string EwbDt { get; set; }
        public string EwbValidTill { get; set; }
        public string QRCodeUrl { get; set; }
        public string EinvoicePdf { get; set; }
        public string Status { get; set; }
        public string alert { get; set; }
        public bool error { get; set; }
    }

    public class Results
    {
        public Message message { get; set; }
        public string errorMessage { get; set; }
        public string InfoDtls { get; set; }
        public string status { get; set; }
        public int code { get; set; }
        public string requestId { get; set; }
    }
}
