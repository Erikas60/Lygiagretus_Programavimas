#include <iostream>
#include <fstream>
#include <cmath>
#include <thread>
#include <string>
#include <condition_variable>
#include <iomanip>
#include <nlohmann/json.hpp>
#include <vector>
#include <iostream>
#include <omp.h>
using namespace std;
using json = nlohmann::json;

struct horse {
    string Name;
    int Age;
    float Speed;
    float Function;
};

class ResultMonitor
{
    

public:
    horse resultHorses[40];
    int count = 0;
    

    static bool compare(const horse& a, const horse& b)
    {
        return a.Function < b.Function;
    }
    

    void addSorted(horse newOne)
    {
#pragma omp critical
        {
            resultHorses[count] = newOne;
            count++;
            std::sort(resultHorses, resultHorses + count, compare);
        }


    }
};

bool Filter(horse horse) {
    
    return horse.Function > 950;
}

float Function(horse horse) 
{
    float ans = 0;
    for (float i = 0; i < horse.Speed; i = i + 0.1)
    {
        for (int j = 0; j < horse.Age; j++)
        {

            if (j % 2 == 0) ans += 3.3;

        }
    }
    
    return ans;
}


int main()
{
    
    ifstream t("IFF-1-8_BriaukaE_L1_dat_2.json");
    
    string jsonFile((istreambuf_iterator<char>(t)), istreambuf_iterator<char>()); // Perskaito .json failą į string

    auto j = json::parse(jsonFile);
    int Size = j["horse"].size();
    auto Horses = new horse[Size];
    int Horsecount = 0;

    
    for (size_t i = 0; i < j["horse"].size(); i++)
    {
        horse tempHorse = {
                j["horse"][i]["Name"],
                j["horse"][i]["Age"],
                j["horse"][i]["Speed"]
        };

        Horses[Horsecount++] = tempHorse;
    }
    int numThreads = std::min(Size / 4,std::max(2, omp_get_max_threads()));
    ResultMonitor result;
    int totalAge = 0;
    float totalSpeed = 0;

#pragma omp parallel reduction(+:totalAge,totalSpeed) num_threads(numThreads) shared(Horses, result)
    {
        int threadId = omp_get_thread_num();
        int chunkSize = Size / numThreads;
        int start = threadId * chunkSize;
        int end = (threadId == numThreads - 1) ? Size : (threadId + 1) * chunkSize;

        for (int i = start; i < end; i++) {

            float function = Function(Horses[i]);
            Horses[i].Function = function;
            if (Filter(Horses[i])) {
                result.addSorted(Horses[i]);
                totalAge += Horses[i].Age;
                totalSpeed += Horses[i].Speed;
            }
        }
    }



    ofstream myfile;
    myfile.open("IFF-1-8_BriaukaE_L1_rez.txt");

    myfile << "-------------------------------------------------------------------------" << endl;
    myfile << "| Name                Age                 Speed               Function  |" << endl;
    myfile << "-------------------------------------------------------------------------" << endl;
    for (int i = 0; i < result.count; i++)
    {
       
        myfile << "| " << left << setw(20) << result.resultHorses[i].Name << left << setw(20) << to_string(result.resultHorses[i].Age) << left << setw(20) <<
            to_string(result.resultHorses[i].Speed) << to_string(result.resultHorses[i].Function) << "| " << + "\n";
    }
    myfile << "-------------------------------------------------------------------------" << endl;
    myfile << "Total Age: " + to_string(totalAge) << endl;
    myfile << "Total Speed: " + to_string(totalSpeed) << endl;
    myfile.close();
    
}

