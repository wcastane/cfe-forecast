using System.Globalization;

var tarifa1C = new PriceRate
{
    Steps = new List<PriceStep>() {
        new PriceStep { Step = Steps.basico, Quantity = 300, Price = 0.876M, Currency = Currency.MXN },
        new PriceStep { Step = Steps.intermedio1, Quantity = 300, Price = 1.0168M, Currency = Currency.MXN },
        new PriceStep { Step = Steps.intermedio2, Quantity = 300, Price = 1.310M, Currency = Currency.MXN },
        new PriceStep { Step = Steps.Excedente, Price = 3.4962M, Currency = Currency.MXN }}
};

//cfeInvoice myInvoice = new cfeInvoice();
//myInvoice.PriceRate = tarifa1C;
//myInvoice.LastRecord = 24728;
//myInvoice.CurrentRecord = 25954;

cfeInvoice myInvoice = new cfeInvoice
{
    PriceRate = tarifa1C,
    StartDate = new DateTime(2024,02,07), 
    EndDate = new DateTime(2024,04,08),
    LastRecord = 6425,
    CurrentRecord = 7087
};

cfeForecast myForecast = new cfeForecast(myInvoice, 8067);

CultureInfo info = new CultureInfo("es-MX");
Console.WriteLine($"Lectura Anterior: {myInvoice.LastRecord} ({myInvoice.StartDate.ToString("d", info)}), Lectura Actual: {myInvoice.CurrentRecord} ({myInvoice.EndDate.ToString("d", info)})");
Console.WriteLine($"Dias consumidos: {myInvoice.InvoiceDays}");
foreach (var item in myInvoice.Details)
{
    Console.WriteLine(String.Format("{0,15} {1,6} {2,20}", item.Step.ToString(), item.Consumed, item.ConsumedValue));
}
Console.WriteLine("{0,15} {1,6} {2,20}","Suma", myInvoice.Consumed, myInvoice.ConsumedValue);
Console.WriteLine("{0,15} {1,6} {2,20}", "Total", "", myInvoice.ConsumedValue * 1.16M);
Console.WriteLine("---------------------------------------------------------");
Console.WriteLine($"Lectura Anterior: {myForecast.LastRecord} ({myForecast.StartDate.ToString("d", info)}), Lectura Actual: {myForecast.CurrentRecord} ({myForecast.EndDate.ToString("d", info)})");
Console.WriteLine($"Dias consumidos: {myForecast.ForcastDays}, Dias restantes: {myForecast.RemainingForcastDays}");
foreach (var item in myForecast.Details)
{
    Console.WriteLine(String.Format("{0,15} {1,6} {2,20}", item.Step.ToString(), item.Consumed, item.ConsumedValue));
}
Console.WriteLine("{0,15} {1,6} {2,20}", "Suma", myForecast.Consumed, myForecast.ConsumedValue);
Console.WriteLine("{0,15} {1,6} {2,20}", "Total", "", myForecast.ConsumedValue * 1.16M);

public enum Steps
{
    basico = 1,
    intermedio1 = 2,
    intermedio2 = 3,
    Excedente  = 9,
}

public enum Currency
{ 
    MXN = 1,
    USD = 2
}

class cfeForecast : cfeInvoice
{
    public cfeForecast(cfeInvoice invoice, int CurrentRecord)
    {
        this.Invoice = invoice;
        this.PriceRate = invoice.PriceRate;
        this.StartDate = invoice.EndDate;
        this.EndDate = invoice.EndDate.AddMonths(2);
        this.LastRecord = invoice.CurrentRecord;
        this.CurrentRecord = CurrentRecord + (this.RemainingForcastDays * ((CurrentRecord - this.LastRecord) / this.ForcastDays));
    }
    public int ForcastDays
    {
        get
        {
            System.TimeSpan diffResult = DateTime.Now.Subtract(this.StartDate);
            return diffResult.Days;
        }
    }
    public int RemainingForcastDays
    {
        get
        {
            System.TimeSpan diffResult = this.EndDate.Subtract(DateTime.Now);
            return diffResult.Days;
        }
    }
    public cfeInvoice Invoice { get; set; }
}

class cfeInvoice
{
    PriceRate _PriceRate;
    List<cfeInvoiceDetail> _Details = new List<cfeInvoiceDetail>();
    public PriceRate PriceRate
    {
        get
        {
            return _PriceRate;
        }
        set
        {
            _PriceRate = value;
            FillDetails(value);
        }
    }

    private void FillDetails(PriceRate value)
    {
        this.Details.Clear();
        int consumed_amount = 0;
        foreach (var item in value.Steps.OrderBy(x => x.Step))
        {
            int consumed = 0;
            if (item.Quantity != 0)
            {
                if (this.Consumed - (consumed_amount + item.Quantity) > 0)
                    consumed = item.Quantity;
                else
                    consumed = this.Consumed - consumed_amount;
                consumed_amount += consumed;
            }
            else
                consumed = this.Consumed - consumed_amount;

            this.Details.Add(new cfeInvoiceDetail
            {
                Step = item.Step,
                Quantity = item.Quantity,
                Price = item.Price,
                Currency = item.Currency,
                Consumed = consumed
            });
        }
    }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int InvoiceDays 
    {
        get
        {
            System.TimeSpan diffResult = this.EndDate.Subtract(this.StartDate);
            return diffResult.Days;
        }
    }
    int _LastRecord = 0;
    public int LastRecord 
    {
        get
        {
            return _LastRecord;
        }
        set 
        { 
            _LastRecord = value;
            if (this.Consumed > 0) FillDetails(PriceRate);
        }  
    }
    int _CurrentRecord = 0;
    public int CurrentRecord 
    {
        get
        {
            return _CurrentRecord;
        }
        set 
        {
            _CurrentRecord = value;
            if (this.Consumed > 0) FillDetails(PriceRate);
        }
    }
    public int Consumed { get => this.CurrentRecord - this.LastRecord; }
    public decimal ConsumedValue { get { return this.Details.Sum(x => x.ConsumedValue); } }
    public List<cfeInvoiceDetail> Details
    {
        get { return _Details; }
        set { _Details = value; }
    }
}
public class cfeInvoiceDetail : PriceStep
{
    public int Consumed { get; set; }
    public decimal ConsumedValue { get { return this.Consumed * this.Price; } }
}

public class PriceRate
{
    public List<PriceStep> Steps { get; set; }
}
public class PriceStep
{
    public Steps Step { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public Currency Currency { get; set; }
}
