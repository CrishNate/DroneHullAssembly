from mlagents_envs.side_channel.side_channel import (
    SideChannel,
    IncomingMessage,
    OutgoingMessage,
)

import uuid
from custom_await import wait_until

class ValidateDesignSideChannel(SideChannel):
    result = False
    received = False

    def __init__(self) -> None:
        super().__init__(uuid.UUID("621f0a70-4f87-11ea-a6bf-784f4387d1f7"))

    def on_message_received(self, msg: IncomingMessage) -> None:
        self.result = msg.read_bool()
        self.received = True

    def reset(self):
        self.result = False
        self.received = False

    def get_result_wait(self):
        if not wait_until(lambda: self.received, 10):
            return False

        return self.result

    def send_string(self, data: str) -> None:
        # Add the string to an OutgoingMessage
        msg = OutgoingMessage()
        msg.write_string(data)
        # We call this method to queue the data we want to send
        super().queue_message_to_send(msg)


