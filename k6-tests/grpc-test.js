import grpc from "k6/net/grpc";
import { sleep } from "k6";

const client = new grpc.Client();
client.load([], "addition.proto");

export const options = {
  stages: [
    { duration: "10m", target: 10000 },
    { duration: "10m", target: 15000 },
    { duration: "30s", target: 0 },
  ],
};

export default function () {
  client.connect("sum-grpc:8080", {
    plaintext: true,
  });

  const a = Math.floor(Math.random() * 100);
  const b = Math.floor(Math.random() * 100);

  const response = client.invoke("Addition/Add", {
    a,
    b,
  });

  if (response.message.result !== a + b) {
    throw new Error(`Unexpected result: ${response.message.result}`);
  }

  client.close();

  sleep(7);
}
