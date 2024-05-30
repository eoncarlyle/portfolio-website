import { inject } from "dioma";
import Backend from "./backend";

const backend = inject(Backend); 
backend.launch();