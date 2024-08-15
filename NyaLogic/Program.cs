// See https://aka.ms/new-console-template for more information
using NyaLogic.Filters;
using NyaLogic.Supported.DSLogic;

// G:\Projects\Асик конфиг\S21\s21dumpsLogic+memDump\S21_PSU+CH0_factory477MhzFull.dsl
// G:\Projects\Асик конфиг\T21\T21-VNISH_PowerSupply-FromStartTo07minOfWork.dsl

if (args.Length > 0)
{
    var F = new Denoise(1);

    var FN = args[0];
    var Measure = DSLogicMeasure.Load(FN);
    for (var i = 0; i < Measure.SequenceCount; i++)
    {
        var Seq = Measure.GetSequence(i);

        F.Apply(Seq);
    }

    Measure.Save($"{FN}.F.dsl");
}