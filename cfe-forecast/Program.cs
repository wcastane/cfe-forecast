var tarifa1C = new PriceRate
{
    Steps = new List<PriceStep>() 
    {
        new PriceStep { Step = Steps.basico, Quantity = 300, Price = 0.801M, Currency = Currency.MXN },
        new PriceStep { Step = Steps.Excedente, Price = 3.082M, Currency = Currency.MXN },
        new PriceStep { Step = Steps.intermedio1, Quantity = 300, Price = 0.926M, Currency = Currency.MXN },
        new PriceStep { Step = Steps.intermedio2, Quantity = 300, Price = 1.198M, Currency = Currency.MXN }
    }
};

//cfeInvoice myInvoice = new cfeInvoice();
//myInvoice.PriceRate = tarifa1C;
//myInvoice.LastRecord = 24728;
//myInvoice.CurrentRecord = 25954;

cfeInvoice myInvoice = new cfeInvoice
{
    PriceRate = tarifa1C,
    LastRecord = 28320,
    CurrentRecord = 28841
};

Console.WriteLine($"Lectura Anterior: {myInvoice.LastRecord}, Lectura Actual: {myInvoice.CurrentRecord}");
foreach (var item in myInvoice.Details)
{
    Console.WriteLine(String.Format("{0,15} {1,6} {2,20}", item.Step.ToString(), item.Consumed, item.ConsumedValue));
}
Console.WriteLine("{0,15} {1,6} {2,20}","Suma", myInvoice.Consumed, myInvoice.ConsumedValue);

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
