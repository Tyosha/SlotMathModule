using System;
using System.Linq;
using Xunit;
using SlotMathModule.GameLogic.Core;
using SlotMathModule.GameLogic.Games;

namespace SlotMathModule.Tests
{
    /// <summary>
    /// Быстрый тест для проверки RTP на 1000 билетов
    /// </summary>
    public class SlotTests_Quick
    {
        [Fact]
        public void Slot_RTP_1000Tickets_Quick()
        {
            // Arrange
            var slot = new Slot();
            slot.LoadGame("Slot5_3");
            
            var feedParams = new FeedExctractParam
            {
                FeedKey = 1000,
                FeedNumber = 1000000,
                TicketNumber = 1000000,
                SegmentNumber = 0,
                GameIndex = 0,
                TicketCount = 1000, // Только 1000 билетов для быстрого теста
                Nominal = 20,
                Percent = 95,
                PercentBonus = 30,
                BonusGameMin = 30,
                BonusGameMax = 500
            };
            
            // ExtractFeed может быть долгим, но не должен висеть
            slot.ExtractFeed(feedParams);
            slot.DoCashIn(10000000);
            slot.Bet = 10;
            slot.Lines = 1;
            
            // Уменьшаем количество спинов для быстрого теста
            int spinCount = 1000; // Вместо 10000
            
            // Act
            for (int i = 0; i < spinCount; i++)
            {
                int result = slot.Spin(10, 1);
                if (result != 0 && result != 301)
                    break;
            }

            // Assert
            Assert.True(slot.SpinCount > 0, $"Должен быть выполнен хотя бы один спин. SpinCount: {slot.SpinCount}");
            Assert.True(slot.CreditIn > 0, $"Должны быть потрачены кредиты. CreditIn: {slot.CreditIn}");
            
            double rtp = 0;
            if (slot.CreditIn > 0)
            {
                rtp = (double)slot.CreditOut / slot.CreditIn * 100;
            }

            // Проверяем, что RTP в разумных пределах (для быстрого теста допускаем широкий диапазон)
            Assert.True(rtp >= 0 && rtp <= 200, 
                $"RTP {rtp:F2}% должен быть в разумных пределах. CreditIn: {slot.CreditIn}, CreditOut: {slot.CreditOut}, SpinCount: {slot.SpinCount}");
        }
    }
}

