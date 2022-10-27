import time

def wait_until(predicate, timeout):
  mustend = time.time() + timeout
  while time.time() < mustend:
    if predicate():
        return True

  return False