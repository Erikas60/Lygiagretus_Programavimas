#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <iostream>
#include <fstream>
#include <vector>
#include "nlohmann/json.hpp"

using json = nlohmann::json;
using namespace std;

const int RESULT_SIZE = 30;

struct Horse {
	char Name[256];
	int Age;
	float Speed;
};

__device__ char toUpperCase(char c) {
	if (c >= 'a' && c <= 'z') {
		return c - ('a' - 'A');
	}
	return c;
}

__device__ const char* getAgeCategory(int age) {
	if (age > 10) {
		return "Senas";
	}
	else {
		return "Jaunas";
	}
}

__device__ void myStrCat(char* dest, const char* src, int destSize) {
	int destLen = 0;
	while (dest[destLen] != '\0' && destLen < destSize) {
		++destLen;
	}

	if (destLen > 0 && destLen < destSize - 1) {
		dest[destLen] = '-';
		++destLen;
	}

	int srcLen = 0;
	if (destLen < destSize - 1 && destLen > 0) {
		dest[destLen] = ' ';
		++destLen;
	}

	while (src[srcLen] != '\0') {
		if (destLen < destSize - 1) {
			dest[destLen] = src[srcLen];
			++destLen;
		}
		++srcLen;
	}
	dest[destLen] = '\0';
}

__global__ void processDataKernel(Horse* horses, char* results, int dataSize, int* index) {
	int threadIndex = threadIdx.x + blockIdx.x * blockDim.x;
	char temp_result[RESULT_SIZE];

	if (threadIndex < dataSize) {
		Horse currentHorse = horses[threadIndex];

		for (int i = 0; i < RESULT_SIZE; ++i) {
			if (currentHorse.Name[i] != '\0') {
				temp_result[i] = toUpperCase(currentHorse.Name[i]);
			}
			else {
				temp_result[i] = ' ';
				break;
			}
		}

		const char* AgeCategory = getAgeCategory(currentHorse.Age);
		myStrCat(temp_result, AgeCategory, RESULT_SIZE);

		if (temp_result[0] == 'X') {
			int writeIndex = atomicAdd(index, RESULT_SIZE);
			for (int i = 0; i < RESULT_SIZE; ++i) {
				results[writeIndex + i] = temp_result[i];
			}
		}
	}
}

int main() {
	// Read data from the file
	std::ifstream inputFile("IFF-1-8_BriaukaE_L1_dat_1.json"); //All data that follows criteria
	
	if (!inputFile.is_open()) {
		std::cerr << "Error opening the input file." << std::endl;
		return 1;
	}

	json jsonData;
	inputFile >> jsonData;
	inputFile.close();

	if (!jsonData.is_array() || jsonData.empty()) {
		std::cerr << "Invalid or empty JSON file." << std::endl;
		return 1;
	}

	std::vector<Horse> horses;
	for (const auto& horseData : jsonData) {
		Horse horse;

		strncpy(horse.Name, horseData["Name"].get<std::string>().c_str(), sizeof(horse.Name) - 1);
		horse.Name[sizeof(horse.Name) - 1] = '\0';

		horse.Age = horseData["Age"];
		horse.Speed = horseData["Speed"];
		horses.push_back(horse);
	}


	int dataSize = horses.size();
	if (dataSize == 0) {
		std::cerr << "No data found in the input file." << std::endl;
		return 1;
	}

	// Allocate memory for horses on the device
	Horse* d_horses;
	cudaMalloc((void**)&d_horses, dataSize * sizeof(Horse));

	// Copy input data from host to device
	cudaMemcpy(d_horses, horses.data(), dataSize * sizeof(Horse), cudaMemcpyHostToDevice);

	// Allocate memory for results on the device
	char* d_results;
	cudaMalloc((void**)&d_results, dataSize * RESULT_SIZE * sizeof(char));
	cudaMemset(d_results, ' ', dataSize * RESULT_SIZE * sizeof(char));

	// Allocate memory for index on the device
	int* d_index;
	cudaMalloc((void**)&d_index, sizeof(int));
	cudaMemset(d_index, 0, sizeof(int));

	// Launch the CUDA kernel
	int threadsperBlock = 32;
	int numBlocks = (dataSize + threadsperBlock - 1) / threadsperBlock;
	processDataKernel<<<numBlocks, threadsperBlock>>>(d_horses, d_results, dataSize, d_index);

	// Copy index from device to host
	int index;
	cudaMemcpy(&index, d_index, sizeof(int), cudaMemcpyDeviceToHost);

	// Copy results from device to host
	char* host_results = new char[index];
	cudaMemcpy(host_results, d_results, index * sizeof(char), cudaMemcpyDeviceToHost);
	cudaDeviceSynchronize();

	//Writing char result to file
	std::ofstream outputFile("IFF-1-8_ErikasB_L1_rez_1.txt");
	if (!outputFile.is_open()) {
		std::cerr << "Error opening the output file." << std::endl;
		return 1;
	}
	for (int i = 0; i < index; i++) {
		char a = host_results[i];
		if (a != '\0') {
			outputFile << a;
		}
		if ((i + 1) % RESULT_SIZE == 0 && i != index - 1) {
			outputFile << "\n";
		}
	}
	outputFile.close();

	// Free allocated memory
	delete[] host_results;
	cudaFree(d_horses);
	cudaFree(d_results);
	cudaFree(d_index);

	return 0;
}