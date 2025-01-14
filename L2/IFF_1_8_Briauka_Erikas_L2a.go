package main

import (
	"encoding/json"
	"fmt"
	"log"
	"os"
	"sort"
)

type DataStructure struct {
	Name  string
	Age   int
	Speed float64
}

type ResultStructure struct {
	Name     string
	Age      int
	Speed    float64
	Function float64
}
type dataMessage struct {
	Operation string
	Horse     DataStructure
}

func main() {

	content, err := os.ReadFile("IFF-1-8_BriaukaE_L1_dat_1.json")
	if err != nil {
		log.Fatal("Error when opening file: ", err)
	}

	var data []DataStructure
	err = json.Unmarshal(content, &data)
	if err != nil {
		log.Fatal("Error during Unmarshal(): ", err)
	}

	dataThreadChannelDataIn := make(chan dataMessage, 1)
	dataThreadChannelDataOut := make(chan DataStructure, 1)
	resultThreadChannelDataIn := make(chan ResultStructure, 1)
	resultThreadChannelDone := make(chan bool)
	resultThreadChannelDataOut := make(chan ResultStructure, 1)
	workerThreadCount := 4

	workerDone := make(chan bool)

	for i := 0; i < workerThreadCount; i++ {
		go func() {

			workerRoutine(dataThreadChannelDataIn, dataThreadChannelDataOut, resultThreadChannelDataIn, workerDone)

		}()
	}

	go dataRoutine(dataThreadChannelDataIn, dataThreadChannelDataOut, len(data)/2)
	go resultRoutine(resultThreadChannelDataIn, resultThreadChannelDataOut, resultThreadChannelDone)

	for i := 0; i < len(data); i++ {
		dataThreadChannelDataIn <- dataMessage{Operation: "add", Horse: data[i]}
	}

	fmt.Println("Main: Waiting for workers to finish")

	for i := 0; i < workerThreadCount; i++ {
		<-workerDone
	}

	fmt.Println("Main: Workers finished")
	close(dataThreadChannelDataIn)

	resultThreadChannelDone <- true
	close(resultThreadChannelDataIn)

	resultsFile, err := os.Create("IFF_1_8_BriaukaErikas_L2a_rez.txt")
	//check(err)

	resultsFile.WriteString(
		"----------------------------------------\n" +
			"| Name     | Age  | Speed | Function    |\n" +
			"----------------------------------------\n")
	for {
		Horse, more := <-resultThreadChannelDataOut
		if more {
			fmt.Fprintf(resultsFile, "|%-10s|%-6d|%-7.2f|%-13.2f|\n", Horse.Name, Horse.Age, Horse.Speed, Horse.Function)
		} else {
			break
		}
	}
	resultsFile.WriteString("----------------------------------------\n")
	defer resultsFile.Close()

}

func workerRoutine(message chan<- dataMessage, dataIn <-chan DataStructure, dataOut chan<- ResultStructure, done chan<- bool) {
	for {
		fmt.Println("Worker: Sending 'remove' message to dataRoutine")
		message <- dataMessage{Operation: "remove", Horse: DataStructure{Name: "Horse", Age: 0, Speed: 0.0}}
		fmt.Println("Worker: Waiting for data from dataRoutine")
		Horse, more := <-dataIn
		if more {
			fmt.Println("Worker: Received data from dataRoutine, processing...")
			function := calculateFunction(Horse)

			if satisfiesCriteria(function) {
				fmt.Println("Worker: Sending result to resultRoutine")
				result := ResultStructure{
					Name:     Horse.Name,
					Age:      Horse.Age,
					Speed:    Horse.Speed,
					Function: function,
				}
				dataOut <- result
			}
		} else {
			fmt.Println("Worker: DataIn channel closed, exiting workerRoutine")
			done <- true
			return
		}
	}

}

func calculateFunction(data DataStructure) float64 {
	var result float64
	for i := 0.0; i < data.Speed; i = i + 0.1 {

		for j := 0; j < data.Age; j++ {

			if j%2 == 0 {
				result += 3.3
			}

		}
	}
	return result
}

func satisfiesCriteria(result float64) bool {

	return result > 950
}

func dataRoutine(dataIn <-chan dataMessage, dataOut chan<- DataStructure, maxSize int) {
	var data []DataStructure

	receivingDone := false
	sendingDone := false

	for {
		fmt.Println("DataRoutine: add/remove operation processed. Data length:", len(data))

		message, more := <-dataIn
		if !more {
			fmt.Println("DataRoutine: DataIn channel closed, breaking loop")
			break
		}

		switch message.Operation {
		case "add":
			if len(data) < maxSize && !receivingDone {
				data = append(data, message.Horse)
			} else {
				receivingDone = true
			}
		case "remove":
			if len(data) > 0 {
				dataOut <- data[len(data)-1]
				data = data[:len(data)-1]
			} else if receivingDone {
				sendingDone = true
			}

		}

		if receivingDone && sendingDone {
			fmt.Println("DataRoutine: Both receivingDone and sendingDone are true, breaking loop")
			break

		}
	}

	fmt.Println("DataRoutine: Closing dataOut channel")
	close(dataOut)
}

func resultRoutine(dataIn <-chan ResultStructure, dataOut chan<- ResultStructure, done <-chan bool) {
	var data []ResultStructure
	receivingDone := false
	for {
		select {
		case <-done:
			receivingDone = true
		case Horse := <-dataIn:
			data = append(data, Horse)
			sort.Slice(data, func(i, j int) bool {
				return data[i].Function < data[j].Function
			})
		}
		if receivingDone {
			break
		}
	}
	for i := 0; i < len(data); i++ {
		dataOut <- data[i]
	}
	fmt.Println("DataRoutine: Exiting for loop")
	close(dataOut)
}
