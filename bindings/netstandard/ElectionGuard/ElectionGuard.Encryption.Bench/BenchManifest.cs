using System;

namespace ElectionGuard.Encryption.Bench
{
    class BenchManifest : Fixture
    {
        public override void Run()
        {
            Bench_Can_Construct_Internationalized_Text();
        }

        public void Bench_Can_Construct_Internationalized_Text()
        {
            Console.WriteLine("Bench_Can_Construct_Internationalized_Text");

            Run(() =>
            {
                var language1 = new Language("some words", "en");
                var language2 = new Language("algunas palabras", "es");
                var languages = new[] { language1, language2 };
                var subject = new InternationalizedText(languages);

                // Assert
                _ = subject.GetTextAt(0);
            });
        }
    }
}
