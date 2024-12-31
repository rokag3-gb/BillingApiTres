///--------------2024/12/30 BillDetail 테이블 사용하지 않음으로 결정.
///--------------BillDetail 테이블의 KeyId와 VenderCode는 BillItem으로 이동.
///--------------하나의 bill item에서 여러 벤더사 데이터 또는 여러 마스터 데이터를 사용하지 않는다

//using Billing.Data.Models.Bill;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Billing.Data.Interfaces
//{
//    public interface IBillDetailRepository
//    {
//        List<BillDetail> GetList(long billId, int? offset, int? limit);
//    }
//}
