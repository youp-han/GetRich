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
        Dictionary<int, int> GetNumbers();
        Lotto_History GetUpdateNumbers();
        int GetTopNumber();
        string GetGetPosNum();

    }
}