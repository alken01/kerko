import { InputOTP, InputOTPGroup, InputOTPSlot } from "./ui/input-otp";

interface TargaInputProps {
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
}

export function TargaInput({ value, onChange, disabled }: TargaInputProps) {
  return (
    <div className="relative flex justify-center items-center">
      <InputOTP
        maxLength={7}
        value={value}
        onChange={onChange}
        disabled={disabled}
        autoComplete="off"
      >
        <InputOTPGroup>
          <InputOTPSlot index={0} />
          <InputOTPSlot index={1} />
        </InputOTPGroup>
        <span className="text-muted-foreground select-none"> </span>
        <InputOTPGroup>
          <InputOTPSlot index={2} />
          <InputOTPSlot index={3} />
          <InputOTPSlot index={4} />
        </InputOTPGroup>
        <span className="text-muted-foreground select-none"> </span>
        <InputOTPGroup>
          <InputOTPSlot index={5} />
          <InputOTPSlot index={6} />
        </InputOTPGroup>
      </InputOTP>
    </div>
  );
}
