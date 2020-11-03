using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lotto.Models;

namespace Lotto.Repository
{
    interface ILottoMngRepository : IDisposable
    {
        List<Lotto_History> GetLottoHistoryList();
        List<Lotto_History> GetLottoHistoryList(int topCounts);
        void LottoNumberSave(Lotto_History item);
        Dictionary<int, int> GetNumberCounts();
        Dictionary<int, int> GetNumberCounts(int recentNumbers);
        Lotto_History GetUpdateNumbers();
        int GetTopNumber();
        string GetGetPosNum();
        List<Numbers_NCounts> ConvertDictionaryToList(Dictionary<int, int> convertedSource);

        List<Target_Numbers> GetTotalCountByPlace();


    }
}